using Confluent.Kafka;
using FluentAssertions;
using Itmo.Dev.Platform.Kafka.Extensions;
using Itmo.Dev.Platform.Kafka.Producer;
using Itmo.Dev.Platform.Kafka.Producer.Outbox;
using Itmo.Dev.Platform.Kafka.Tests.Extensions;
using Itmo.Dev.Platform.Kafka.Tests.Fixtures;
using Itmo.Dev.Platform.Kafka.Tests.Outbox;
using Itmo.Dev.Platform.Kafka.Tools;
using Itmo.Dev.Platform.MessagePersistence;
using Itmo.Dev.Platform.MessagePersistence.Internal.Models;
using Itmo.Dev.Platform.MessagePersistence.Internal.Persistence;
using Itmo.Dev.Platform.MessagePersistence.Options;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Extensions;
using Itmo.Dev.Platform.Testing.ApplicationFactories;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Data;
using Xunit;
using Xunit.Abstractions;

namespace Itmo.Dev.Platform.Kafka.Tests;

#pragma warning disable CA1506

[Collection(nameof(KafkaCollectionFixture))]
public class KafkaOutboxTests : IAsyncLifetime, IClassFixture<KafkaDatabaseFixture>
{
    private const string TopicName = $"{nameof(KafkaProducerTests)}_topic";

    private readonly KafkaFixture _kafkaFixture;
    private readonly KafkaDatabaseFixture _databaseFixture;

    public KafkaOutboxTests(KafkaFixture kafkaFixture, ITestOutputHelper output, KafkaDatabaseFixture databaseFixture)
    {
        _kafkaFixture = kafkaFixture;
        _databaseFixture = databaseFixture;

        Log.Logger = new LoggerConfiguration()
            .WriteTo.TestOutput(output)
            .CreateLogger();
    }

    [Theory]
    [MemberData(nameof(GetMessages))]
    public async Task ProduceAsync_ShouldWriteMessage(KafkaOutboxTestData testData, KafkaOutboxConfigData configData)
    {
        // Arrange
        await using var fixtureScope = _databaseFixture.Scope;

        await using var application = new PlatformWebApplicationBuilder<Program>()
            .AddConfigurationJson(
                $$"""
            {
              "MessagePersistence": {
                "SchemaName": "message_persistence",
                "Publisher": {
                  "Default": {
                    "{{nameof(MessagePersistencePublisherOptions.BatchSize)}}": "{{testData.BufferSize}}",
                    "{{nameof(MessagePersistencePublisherOptions.PollingDelay)}}": "00:00:00.500"
                  }
                }
              },
              "Producer": {
                "{{nameof(KafkaProducerOptions.Topic)}}": "{{TopicName}}"
              }
            }
            """)
            .ConfigureConfiguration(configData.ApplyConfig)
            .ConfigureServices(collection => collection.AddPlatformMessagePersistence(builder => builder
                .WithDefaultPublisherOptions("MessagePersistence:Publisher:Default")
                .UsePostgresPersistence(configurator => configurator.ConfigureOptions("MessagePersistence")))
            )
            .ConfigureServices((collection, configuration) => collection.AddPlatformKafka(builder => builder
                .ConfigureTestOptions(_kafkaFixture.Host)
                .AddProducer(b => b
                    .WithKey<int>()
                    .WithValue<string>()
                    .WithConfiguration(configuration.GetSection("Producer"))
                    .SerializeKeyWithNewtonsoft()
                    .SerializeValueWithNewtonsoft()
                    .WithOutbox())))
            .ConfigureServices(collection => _databaseFixture.AddPlatformPersistence(collection))
            .Build();

        application.CreateClient();

        await using var innerScope = application.Services.CreateAsyncScope();
        var provider = innerScope.ServiceProvider;

        var producer = provider.GetRequiredService<IKafkaMessageProducer<int, string>>();

        var consumerConfig = new ConsumerConfig
        {
            GroupId = TestConsumerGroup.GetName(testData.TestKey),
            BootstrapServers = _kafkaFixture.Host,
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        using var consumer = new ConsumerBuilder<int, string>(consumerConfig)
            .SetKeyDeserializer(new NewtonsoftJsonValueSerializer<int>())
            .SetValueDeserializer(new NewtonsoftJsonValueSerializer<string>())
            .Build();

        consumer.Subscribe(TopicName);

        await producer.ProduceAsync(testData.Messages.ToAsyncEnumerable(), default);

        // Act

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(5 * testData.Messages.Count));

        // Assert
        producer.Should().BeOfType(configData.ExpectedProducerType);

        var consumedMessages = new List<Confluent.Kafka.Message<int, string>>();

        while (cts.IsCancellationRequested is false && consumedMessages.Count != testData.Messages.Count)
        {
            var result = consumer.Consume(cts.Token);

            if (result.Message.Key == testData.TestKey)
            {
                consumedMessages.Add(result.Message);
                Log.Information("Adding result with value = {Value}", result.Message.Value);
            }
            else
            {
                Log.Information("Skipping result with key = {Key}", result.Message.Key);
            }
        }

        cts.IsCancellationRequested.Should().BeFalse("Failed to wait for all message consumption");

        consumedMessages.Should().HaveCount(testData.Messages.Count);
        consumedMessages = consumedMessages.OrderBy(x => x.Value).ToList();

        consumedMessages.Zip(testData.Messages)
            .Should()
            .AllSatisfy(tuple => tuple.First
                .Should()
                .BeEquivalentTo(
                    tuple.Second,
                    opt => opt
                        .Including(x => x.Key)
                        .Including(x => x.Value)));

        // Delay so we do not intersect with FOR UPDATE SKIP LOCKED in initial publisher transaction
        await Task.Delay(TimeSpan.FromMilliseconds(100), cts.Token);

        var outboxRepository = provider.GetRequiredService<IMessagePersistenceInternalRepository>();

        var query = PersistedMessageQuery.Build(builder => builder
            .WithPageSize(int.MaxValue)
            .WithName(KafkaOutboxMessageName.ForTopic(TopicName))
            .WithCursor(DateTimeOffset.MinValue));

        var outboxMessages = await outboxRepository
            .QueryAsync(query, default)
            .ToArrayAsync();

        if (configData.ShouldWriteOutboxMessages)
        {
            outboxMessages.Should().HaveCount(testData.Messages.Count);
            outboxMessages.Should().AllSatisfy(x => x.State.Should().Be(MessageState.Completed));
        }
        else
        {
            outboxMessages.Should().BeEmpty();
        }

        // Dispose
        consumer.Close();
    }

    public static IEnumerable<object[]> GetMessages()
    {
        KafkaOutboxConfigData[] configs =
        [
            new(OutboxStrategy: null,
                typeof(AlwaysOutboxMessageProducer<int, string>),
                ShouldWriteOutboxMessages: true),

            new(OutboxStrategy.Always,
                typeof(AlwaysOutboxMessageProducer<int, string>),
                ShouldWriteOutboxMessages: true),

            new(OutboxStrategy.Fallback,
                typeof(FallbackOutboxMessageProducer<int, string>),
                ShouldWriteOutboxMessages: false),
        ];

        return configs.SelectMany(
            _ => new[]
            {
                KafkaOutboxTestData.SingleMessage(1),
                KafkaOutboxTestData.SingleMessage(10),
                KafkaOutboxTestData.Many(1, 10),
                KafkaOutboxTestData.Many(5, 10),
                KafkaOutboxTestData.Many(10, 10),
            },
            static (config, data) => new object[] { data, config });
    }

    public async Task InitializeAsync()
    {
        await _kafkaFixture.CreateTopicsAsync(TopicName);
        await _kafkaFixture.ClearTopicsAsync(TopicName);
    }

    public async Task DisposeAsync()
    {
        const string truncateSql = """
        delete from message_persistence.persisted_messages;
        """;

        await using var command = _databaseFixture.Connection.CreateCommand();
        command.CommandText = truncateSql;

        if (_databaseFixture.Connection.State is not ConnectionState.Open)
        {
            await _databaseFixture.Connection.OpenAsync();
        }

        await command.ExecuteNonQueryAsync();

        await _databaseFixture.ResetAsync();
        await _kafkaFixture.ClearTopicsAsync(TopicName);
    }
}
