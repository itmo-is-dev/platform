using Confluent.Kafka;
using FluentAssertions;
using Itmo.Dev.Platform.Kafka.Consumer;
using Itmo.Dev.Platform.Kafka.Extensions;
using Itmo.Dev.Platform.Kafka.Tests.Extensions;
using Itmo.Dev.Platform.Kafka.Tests.Fixtures;
using Itmo.Dev.Platform.Kafka.Tests.Tools;
using Itmo.Dev.Platform.Kafka.Tools;
using Itmo.Dev.Platform.MessagePersistence;
using Itmo.Dev.Platform.MessagePersistence.Options;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Configuration;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Extensions;
using Itmo.Dev.Platform.Testing.ApplicationFactories;
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

        await using var application = new PlatformWebApplicationBuilder<Program>()
            .AddConfigurationJson(
                $$"""
            {
                "MessagePersistence": {
                    "{{nameof(MessagePersistencePostgresOptions.SchemaName)}}": "message_persistence",
                    "Publisher": {
                        "Default": {
                            "{{nameof(MessagePersistencePublisherOptions.BatchSize)}}": {{bufferSize}},
                            "{{nameof(MessagePersistencePublisherOptions.PollingDelay)}}": "00:00:00.500"
                        }
                    }
                },
                "Consumer": {
                    "{{nameof(KafkaConsumerOptions.Topic)}}": "{{TopicName}}",
                    "{{nameof(KafkaConsumerOptions.Group)}}": "{{nameof(KafkaInboxTests)}}",
                    "{{nameof(KafkaConsumerOptions.InstanceId)}}": "{{nameof(KafkaInboxTests)}}",
                    "{{nameof(KafkaConsumerOptions.BufferWaitLimit)}}": "00:00:00.200",
                    "{{nameof(KafkaConsumerOptions.BufferSize)}}": "{{bufferSize}}"
                }
            }
            """)
            .ConfigureServices(x => x.AddSingleton(testContext))
            .ConfigureServices(collection => collection
                .AddPlatformMessagePersistence(builder => builder
                    .WithDefaultPublisherOptions("MessagePersistence:Publisher:Default")
                    .UsePostgresPersistence(configurator => configurator.ConfigureOptions("MessagePersistence")))
            )
            .ConfigureServices((collection, configuration) => collection.AddPlatformKafka(builder => builder
                .ConfigureTestOptions(_kafkaFixture.Host)
                .AddConsumer(b => b
                    .WithKey<int>()
                    .WithValue<string>()
                    .WithConfiguration(configuration.GetSection("Consumer"))
                    .DeserializeKeyWithNewtonsoft()
                    .DeserializeValueWithNewtonsoft()
                    .HandleInboxWith<CollectionInboxHandler<int, string>>())))
            .ConfigureServices(collection => _databaseFixture.AddPlatformPersistence(collection))
            .Build();

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
        cts.CancelAfter(TimeSpan.FromSeconds(5 * messages.Length));

        while (testContext.Messages.Count != messages.Length && cts.IsCancellationRequested is false)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100), CancellationToken.None);
        }

        Log.Information("Expected count = {Count}", messages.Length);
        Log.Information("Actual count = {Count}", testContext.Messages.Count);

        // Assert
        testContext.Messages.Zip(messages)
            .Should()
            .AllSatisfy(tuple => tuple.First
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
