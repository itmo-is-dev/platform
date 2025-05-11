using FluentAssertions;
using Itmo.Dev.Platform.Common.Lifetime;
using Itmo.Dev.Platform.MessagePersistence.Extensions;
using Itmo.Dev.Platform.MessagePersistence.Models;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Extensions;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Repositories;
using Itmo.Dev.Platform.MessagePersistence.Tests.Fixtures;
using Itmo.Dev.Platform.MessagePersistence.Tests.Handlers;
using Itmo.Dev.Platform.MessagePersistence.Tests.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
    public async Task ConsumeAsync_ShouldDeliverMessagesToHandler()
    {
        // Arrange
        await using var fixtureScope = _database.Scope;
        var context = new TestContext<int, string>();

        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(
                builder => builder.ConfigureServices(
                    collection =>
                    {
                        var configuration = new ConfigurationManager();

                        configuration.AddInMemoryCollection(
                            new Dictionary<string, string?>
                            {
                                ["Handler:BatchSize"] = "10",
                                ["Handler:PollingDelay"] = "00:00:01",
                            });

                        collection.AddPlatformMessagePersistence(
                            b => b
                                .UsePostgresPersistence(
                                    configurator => configurator.ConfigureOptions(
                                        options => options.Configure(o => o.SchemaName = "message_persistence")))
                                .AddMessage(
                                    m => m
                                        .Called(nameof(MessagePersistenceHandlerTests))
                                        .WithConfiguration(configuration.GetSection("Handler"))
                                        .WithKey<int>()
                                        .WithValue<string>()
                                        .HandleBy<TestPersistenceHandler<int, string>>()));

                        _database.AddPlatformPersistence(collection);

                        collection.AddSingleton(context);

                        collection.AddLogging(b => b.AddSerilog());
                    }));

        application.CreateClient();

        var platformLifetime = application.Services.GetRequiredService<IPlatformLifetime>();
        await platformLifetime.WaitOnInitializedAsync(default);

        await using var scope = application.Services.CreateAsyncScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IMessagePersistenceConsumer>();

        var message = new PersistedMessage<int, string>(1, "aboba");

        // Act
        await consumer.ConsumeAsync(nameof(MessagePersistenceHandlerTests), new[] { message }, default);

        var timeout = Task.Delay(TimeSpan.FromSeconds(30));
        await Task.WhenAny(context.Message, timeout);

        // Assert
        context.Message.IsCompletedSuccessfully.Should().BeTrue();
        context.Message.Result.Should().BeEquivalentTo(message);
    }

    [Fact]
    public async Task ConsumeAsync_ShouldFailMessage_WhenRetryCountExceeded()
    {
        await using var fixtureScope = _database.Scope;
        var context = new TestCollectionContext<int, string>();

        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(
                builder => builder.ConfigureServices(
                    collection =>
                    {
                        var configuration = new ConfigurationManager();

                        configuration.AddInMemoryCollection(
                            new Dictionary<string, string?>
                            {
                                ["Handler:BatchSize"] = "10",
                                ["Handler:PollingDelay"] = "00:00:01",
                                ["Handler:RetryCount"] = "2",
                            });

                        collection.AddPlatformMessagePersistence(
                            b => b
                                .UsePostgresPersistence(
                                    configurator => configurator.ConfigureOptions(
                                        options => options.Configure(o => o.SchemaName = "message_persistence")))
                                .AddMessage(
                                    m => m
                                        .Called(nameof(MessagePersistenceHandlerTests))
                                        .WithConfiguration(configuration.GetSection("Handler"))
                                        .WithKey<int>()
                                        .WithValue<string>()
                                        .HandleBy<FailingHandler<int, string>>()));

                        _database.AddPlatformPersistence(collection);

                        collection.AddSingleton(context);

                        collection.AddLogging(b => b.AddSerilog());
                    }));

        application.CreateClient();

        var platformLifetime = application.Services.GetRequiredService<IPlatformLifetime>();
        await platformLifetime.WaitOnInitializedAsync(default);

        await using var scope = application.Services.CreateAsyncScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IMessagePersistenceConsumer>();

        var message = new PersistedMessage<int, string>(1, "aboba");

        // Act
        await consumer.ConsumeAsync(nameof(MessagePersistenceHandlerTests), new[] { message }, default);

        var timeout = Task.Delay(TimeSpan.FromSeconds(30));

        while (timeout.IsCompletedSuccessfully is false && context.Messages.Count < 2)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(200));
        }

        // Assert
        context.Messages.Count.Should().Be(2);
        await Task.Delay(TimeSpan.FromSeconds(1));

        var repository = scope.ServiceProvider.GetRequiredService<MessagePersistenceRepository>();

        var query = SerializedMessageQuery.Build(
            x => x
                .WithName(nameof(MessagePersistenceHandlerTests))
                .WithState(MessageState.Failed)
                .WithPageSize(1));

        var failedMessage = await repository
            .QueryAsync(query, default)
            .SingleOrDefaultAsync();

        failedMessage.Should().NotBeNull();
        failedMessage!.RetryCount.Should().Be(2);
    }
}