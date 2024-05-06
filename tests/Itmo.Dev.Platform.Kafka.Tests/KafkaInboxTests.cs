using Confluent.Kafka;
using FluentAssertions;
using Itmo.Dev.Platform.Kafka.Consumer;
using Itmo.Dev.Platform.Kafka.Extensions;
using Itmo.Dev.Platform.Kafka.Tests.Extensions;
using Itmo.Dev.Platform.Kafka.Tests.Fixtures;
using Itmo.Dev.Platform.Kafka.Tests.Tools;
using Itmo.Dev.Platform.Kafka.Tools;
using Itmo.Dev.Platform.MessagePersistence.Configuration;
using Itmo.Dev.Platform.MessagePersistence.Extensions;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Itmo.Dev.Platform.Kafka.Tests;

#pragma warning disable CA1506

[Collection(nameof(KafkaCollectionFixture))]
public class KafkaInboxTests : IAsyncLifetime, IClassFixture<KafkaDatabaseFixture>
{
    private const string TopicName = $"{nameof(KafkaInboxTests)}_topic";

    private readonly KafkaFixture _kafkaFixture;
    private readonly KafkaDatabaseFixture _databaseFixture;

    public KafkaInboxTests(KafkaFixture kafkaFixture, ITestOutputHelper output, KafkaDatabaseFixture databaseFixture)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.TestOutput(output)
            .CreateLogger();

        _kafkaFixture = kafkaFixture;
        _databaseFixture = databaseFixture;
    }

    [Theory]
    [MemberData(nameof(GetMessages))]
    public async Task Consume_ShouldConsume_WhenMessageProducedToInbox(int bufferSize, Message<int, string>[] messages)
    {
        // Arrange
        await using var fixtureScope = _databaseFixture.Scope;
        var testContext = new TestContext<int, string>();

        void ConfigureAppConfiguration(IConfigurationBuilder configuration)
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["MessagePersistence:SchemaName"] = "message_persistence",
                    [$"Consumer:{nameof(KafkaConsumerOptions.Topic)}"] = TopicName,
                    [$"Consumer:{nameof(KafkaConsumerOptions.Group)}"] = nameof(KafkaInboxTests),
                    [$"Consumer:{nameof(KafkaConsumerOptions.InstanceId)}"] = nameof(KafkaInboxTests),
                    [$"Consumer:{nameof(KafkaConsumerOptions.BufferWaitLimit)}"] = "00:00:00.200",
                    [$"Consumer:{nameof(KafkaConsumerOptions.BufferSize)}"] = bufferSize.ToString(),
                    [$"Consumer:Inbox:{nameof(MessagePersistenceHandlerOptions.BatchSize)}"] = bufferSize.ToString(),
                    [$"Consumer:Inbox:{nameof(MessagePersistenceHandlerOptions.PollingDelay)}"] = "00:00:00.500",
                });
        }

        void ConfigureServices(IServiceCollection collection, IConfiguration configuration)
        {
            collection.AddSingleton(testContext);

            collection.AddPlatformMessagePersistence(
                builder => builder
                    .UsePostgresPersistence(
                        configurator => configurator.ConfigureOptions(
                            b => b.Bind(configuration.GetSection("MessagePersistence")))));

            collection.AddPlatformKafka(
                builder => builder
                    .ConfigureTestOptions(_kafkaFixture.Host)
                    .AddConsumer(
                        b => b
                            .WithKey<int>()
                            .WithValue<string>()
                            .WithConfiguration(configuration.GetSection("Consumer"))
                            .DeserializeKeyWithNewtonsoft()
                            .DeserializeValueWithNewtonsoft()
                            .HandleInboxWith<CollectionInboxHandler<int, string>>()));

            _databaseFixture.AddPlatformPersistence(collection);

            collection.AddLogging(x => x.AddSerilog());
            collection.AddOptions();
        }

        await using var application = new WebApplicationFactory<Program>().WithWebHostBuilder(
            hb => hb
                .ConfigureAppConfiguration((_, configuration) => ConfigureAppConfiguration(configuration))
                .ConfigureServices((context, collection) => ConfigureServices(collection, context.Configuration)));

        application.CreateClient();

        var config = new ProducerConfig
        {
            BootstrapServers = _kafkaFixture.Host,
            MessageMaxBytes = 1_000_000,
        };

        using var producer = new ProducerBuilder<int, string>(config)
            .SetKeySerializer(new NewtonsoftJsonValueSerializer<int>())
            .SetValueSerializer(new NewtonsoftJsonValueSerializer<string>())
            .Build();

        foreach (Message<int, string> message in messages)
        {
            await producer.ProduceAsync(TopicName, message);
        }

        // Act

        using var cts = new CancellationTokenSource();
        // cts.CancelAfter(TimeSpan.FromSeconds(5 * messages.Length));

        while (testContext.Messages.Count != messages.Length && cts.IsCancellationRequested is false)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100), CancellationToken.None);
        }

        Log.Information("Expected count = {Count}", messages.Length);
        Log.Information("Actual count = {Count}", testContext.Messages.Count);

        // Assert
        testContext.Messages.Zip(messages)
            .Should()
            .AllSatisfy(
                tuple => tuple.First
                    .Should()
                    .BeEquivalentTo(
                        tuple.Second,
                        opt => opt
                            .Including(x => x.Key)
                            .Including(x => x.Value)));
    }

    public static IEnumerable<object[]> GetMessages()
    {
        yield return
        [
            1,
            new object[]
            {
                new Message<int, string>
                {
                    Key = 1,
                    Value = "aboba",
                },
            },
        ];

        yield return
        [
            10,
            new object[]
            {
                new Message<int, string>
                {
                    Key = 1,
                    Value = "aboba",
                },
            },
        ];

        yield return
        [
            1,
            Enumerable
                .Range(0, 10)
                .Select(i => new Message<int, string> { Key = i, Value = i.ToString() })
                .ToArray(),
        ];

        yield return
        [
            5,
            Enumerable
                .Range(0, 10)
                .Select(i => new Message<int, string> { Key = i, Value = i.ToString() })
                .ToArray(),
        ];

        yield return
        [
            10,
            Enumerable
                .Range(0, 10)
                .Select(i => new Message<int, string> { Key = i, Value = i.ToString() })
                .ToArray(),
        ];
    }

    public async Task InitializeAsync()
    {
        await _kafkaFixture.CreateTopicsAsync(TopicName);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}