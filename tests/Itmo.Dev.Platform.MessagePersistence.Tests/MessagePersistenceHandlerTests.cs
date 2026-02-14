using FluentAssertions;
using Itmo.Dev.Platform.Common.Lifetime;
using Itmo.Dev.Platform.MessagePersistence.Internal.Metrics;
using Itmo.Dev.Platform.MessagePersistence.Internal.Models;
using Itmo.Dev.Platform.MessagePersistence.Internal.Persistence;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Extensions;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Repositories;
using Itmo.Dev.Platform.MessagePersistence.Tests.Fixtures;
using Itmo.Dev.Platform.MessagePersistence.Tests.Handlers;
using Itmo.Dev.Platform.MessagePersistence.Tests.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Serilog;
using Xunit;

namespace Itmo.Dev.Platform.MessagePersistence.Tests;

#pragma warning disable CA1506

[Collection(nameof(MessagePersistenceCollectionFixture))]
public class MessagePersistenceHandlerTests
{
    private readonly MessagePersistenceDatabaseFixture _database;

    public MessagePersistenceHandlerTests(MessagePersistenceDatabaseFixture database)
    {
        _database = database;
    }

    [Fact]
    public async Task PersistAsync_ShouldDeliverMessagesToHandler()
    {
        // Arrange
        await using var fixtureScope = _database.Scope;
        var context = new TestContext<int, int>();

        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.ConfigureServices(collection =>
            {
                var configuration = new ConfigurationManager();

                configuration.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["Handler:BatchSize"] = "10",
                        ["Handler:PollingDelay"] = "00:00:01",
                    });

                collection.AddPlatformMessagePersistence(b => b
                    .WithDefaultPublisherOptions(options => options.Bind(configuration.GetSection("Handler")))
                    .UsePostgresPersistence(configurator
                        => configurator.ConfigureOptions(options
                            => options.Configure(o => o.SchemaName = "message_persistence")))
                    .AddMessage(m => m
                        .Called(nameof(PersistAsync_ShouldDeliverMessagesToHandler))
                        .WithConfiguration(configuration.GetSection("Handler"))
                        .WithMessage<TestPersistedMessage<int, int>>()
                        .HandleBy<TestPersistedMessageHandler<int, int>>()));

                _database.AddPlatformPersistence(collection);

                collection.AddSingleton(context);

                collection.AddLogging(b => b.AddSerilog());
            }));

        application.CreateClient();

        var platformLifetime = application.Services.GetRequiredService<IPlatformLifetime>();
        await platformLifetime.WaitOnInitializedAsync(default);

        await using var scope = application.Services.CreateAsyncScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IMessagePersistenceService>();

        var message = TestPersistenceMessage.Create(1, 1);

        // Act
        await consumer.PersistAsync([message], default);

        var timeout = Task.Delay(TimeSpan.FromSeconds(30));
        await Task.WhenAny(context.Message, timeout);

        // Assert
        context.Message.IsCompletedSuccessfully.Should().BeTrue();
        context.Message.Result.Should().BeEquivalentTo(message.Payload);
    }

    [Fact]
    public async Task PersistAsync_ShouldFailMessage_WhenRetryCountExceeded()
    {
        await using var fixtureScope = _database.Scope;
        var context = new TestCollectionContext<int, int>();

        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.ConfigureServices(collection =>
            {
                var configuration = new ConfigurationManager();

                configuration.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["Handler:BatchSize"] = "10",
                        ["Handler:PollingDelay"] = "00:00:01",
                        ["Handler:RetryCount"] = "2",
                    });

                collection.AddPlatformMessagePersistence(b => b
                    .WithDefaultPublisherOptions(options => options.Bind(configuration.GetSection("Handler")))
                    .UsePostgresPersistence(configurator
                        => configurator.ConfigureOptions(options
                            => options.Configure(o => o.SchemaName = "message_persistence")))
                    .AddMessage(m => m
                        .Called(nameof(PersistAsync_ShouldFailMessage_WhenRetryCountExceeded))
                        .WithConfiguration(configuration.GetSection("Handler"))
                        .WithMessage<TestPersistedMessage<int, int>>()
                        .HandleBy<FailingHandler<int, int>>()));

                _database.AddPlatformPersistence(collection);

                collection.AddSingleton(context);

                collection.AddLogging(b => b.AddSerilog());
            }));

        application.CreateClient();

        var platformLifetime = application.Services.GetRequiredService<IPlatformLifetime>();
        await platformLifetime.WaitOnInitializedAsync(default);

        await using var scope = application.Services.CreateAsyncScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IMessagePersistenceService>();

        var message = TestPersistenceMessage.Create(1, 1);

        // Act
        await consumer.PersistAsync([message], default);

        var timeout = Task.Delay(TimeSpan.FromSeconds(30));

        while (timeout.IsCompletedSuccessfully is false && context.Messages.Count < 2)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(200));
        }

        // Assert
        context.Messages.Count.Should().Be(2);
        await Task.Delay(TimeSpan.FromSeconds(1));

        var repository = scope.ServiceProvider.GetRequiredService<MessagePersistenceRepository>();

        var query = PersistedMessageQuery.Build(x => x
            .WithName(nameof(PersistAsync_ShouldFailMessage_WhenRetryCountExceeded))
            .WithState(MessageState.Failed)
            .WithPageSize(1));

        var failedMessage = await repository
            .QueryAsync(query, default)
            .SingleOrDefaultAsync();

        failedMessage.Should().NotBeNull();
        failedMessage!.RetryCount.Should().Be(2);
    }

    [Fact]
    public async Task PersistAsync_ShouldDeliverMessageAndUpdateVersion_WhenOldVersionIsPersisted()
    {
        // Arrange
        await using var fixtureScope = _database.Scope;
        var context = new TestContext<int, int>();

        var metrics = new Mock<IMessagePersistenceMetrics>(MockBehavior.Strict);

        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.ConfigureServices(collection =>
            {
                var configuration = new ConfigurationManager();

                configuration.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["Handler:BatchSize"] = "10",
                        ["Handler:PollingDelay"] = "00:00:01",
                    });

                collection.AddPlatformMessagePersistence(b => b
                    .WithDefaultPublisherOptions(options => options.Bind(configuration.GetSection("Handler")))
                    .UsePostgresPersistence(configurator
                        => configurator.ConfigureOptions(options
                            => options.Configure(o => o.SchemaName = "message_persistence")))
                    .AddMessage(m => m
                        .Called(nameof(PersistAsync_ShouldDeliverMessageAndUpdateVersion_WhenOldVersionIsPersisted))
                        .WithConfiguration(configuration.GetSection("Handler"))
                        .WithMessage<TestVersionedPersistedMessage<int, int>>()
                        .HandleBy<TestVersionedPersistedMessageHandler<int, int>>()));

                _database.AddPlatformPersistence(collection);

                collection.AddSingleton(context);

                collection.AddLogging(b => b.AddSerilog());

                collection.Replace(ServiceDescriptor.Singleton(metrics.Object));
            }));

        application.CreateClient();

        var platformLifetime = application.Services.GetRequiredService<IPlatformLifetime>();
        await platformLifetime.WaitOnInitializedAsync(default);

        await using var scope = application.Services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMessagePersistenceInternalRepository>();

        var message = new PersistedMessageCreateModel
        {
            Name = nameof(PersistAsync_ShouldDeliverMessageAndUpdateVersion_WhenOldVersionIsPersisted),
            Version = PayloadVersion.Default,
            Key = "1",
            Value = """
                {
                    "Key": 1,
                    "Value": 1
                }
                """,
            CreatedAt = DateTimeOffset.UtcNow,
            State = MessageState.Pending,
            Headers = new Dictionary<string, string>(),
        };

        metrics.SetupSequence(x => x.IncMessagePayloadMigrated(
            nameof(PersistAsync_ShouldDeliverMessageAndUpdateVersion_WhenOldVersionIsPersisted),
            0,
            1));

        // Act
        await repository.AddAsync([message], default).AsTask(default);

        var timeout = Task.Delay(TimeSpan.FromSeconds(30));
        await Task.WhenAny(context.Message, timeout);

        // Assert
        context.Message.IsCompletedSuccessfully.Should().BeTrue();
        context.Message.Result.Should().BeEquivalentTo(new TestMessage<int, int>(1, 1));
        metrics.VerifyAll();
    }
}
