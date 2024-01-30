using FluentAssertions;
using Itmo.Dev.Platform.MessagePersistence.Extensions;
using Itmo.Dev.Platform.MessagePersistence.Tests.Fixtures;
using Itmo.Dev.Platform.MessagePersistence.Tests.Handlers;
using Itmo.Dev.Platform.MessagePersistence.Tests.Models;
using Itmo.Dev.Platform.Postgres.Extensions;
using Itmo.Dev.Platform.Postgres.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
            .WithWebHostBuilder(builder => builder.ConfigureServices(collection =>
            {
                var configuration = new ConfigurationManager();

                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Handler:BatchSize"] = "10",
                    ["Handler:PollingDelay"] = "00:00:01",
                });

                collection.AddPlatformMessagePersistence(b => b
                    .ConfigurePersistence(configuration, o => o.SchemaName = "message_persistence")
                    .AddMessage(m => m
                        .Called(nameof(MessagePersistenceHandlerTests))
                        .WithConfiguration(configuration.GetSection("Handler"))
                        .WithKey<int>()
                        .WithValue<string>()
                        .HandleBy<TestPersistenceHandler<int, string>>()));

                // ReSharper disable once AccessToDisposedClosure
                var connectionString = fixtureScope.ServiceProvider.GetRequiredService<PostgresConnectionString>();

                collection.AddPlatformPostgres(_ => { });
                collection.RemoveAll<PostgresConnectionString>();
                collection.AddSingleton(connectionString);

                collection.AddSingleton(context);

                collection.AddLogging(b => b.AddSerilog());
            }));

        application.CreateClient();

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
}