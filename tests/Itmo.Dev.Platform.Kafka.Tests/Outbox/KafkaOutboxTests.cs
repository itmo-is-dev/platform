using Confluent.Kafka;
using FluentAssertions;
using Itmo.Dev.Platform.Kafka.Extensions;
using Itmo.Dev.Platform.Kafka.Producer;
using Itmo.Dev.Platform.Kafka.Producer.Outbox;
using Itmo.Dev.Platform.Kafka.Tests.Extensions;
using Itmo.Dev.Platform.Kafka.Tests.Fixtures;
using Itmo.Dev.Platform.Kafka.Tests.Outbox.Models;
using Itmo.Dev.Platform.Kafka.Tools;
using Itmo.Dev.Platform.MessagePersistence;
using Itmo.Dev.Platform.MessagePersistence.Configuration;
using Itmo.Dev.Platform.MessagePersistence.Extensions;
using Itmo.Dev.Platform.MessagePersistence.Models;
using Itmo.Dev.Platform.MessagePersistence.Persistence;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Data;
using Xunit;
using Xunit.Abstractions;

namespace Itmo.Dev.Platform.Kafka.Tests.Outbox;

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

        void ConfigureAppConfiguration(IConfigurationBuilder configuration)
        {
            var dictionary = new Dictionary<string, string?>
            {
                ["MessagePersistence:SchemaName"] = "message_persistence",
                [$"Producer:{nameof(KafkaProducerOptions.Topic)}"] = TopicName,
                [$"Producer:Outbox:{nameof(MessagePersistenceHandlerOptions.BatchSize)}"] = testData.BufferSizeString,
                [$"Producer:Outbox:{nameof(MessagePersistenceHandlerOptions.PollingDelay)}"] = "00:00:00.500",
            };

            configData.ApplyConfig(dictionary);
            configuration.AddInMemoryCollection(dictionary);
        }

        void ConfigureServices(IServiceCollection collection, IConfiguration configuration)
        {
            collection.AddPlatformMessagePersistence(
                builder => builder
                    .UsePostgresPersistence(
                        configurator => configurator.ConfigureOptions(
                            b => b.Bind(configuration.GetSection("MessagePersistence")))));

            collection.AddPlatformKafka(
                builder => builder
                    .ConfigureTestOptions(_kafkaFixture.Host)
                    .AddProducer(
                        b => b
                            .WithKey<int>()
                            .WithValue<string>()
                            .WithConfiguration(configuration.GetSection("Producer"))
                            .SerializeKeyWithNewtonsoft()
                            .SerializeValueWithNewtonsoft()
                            .WithOutbox()));

            _databaseFixture.AddPlatformPersistence(collection);

            collection.AddLogging(x => x.AddSerilog());
            collection.AddOptions();
        }

        await using var application = new WebApplicationFactory<Program>().WithWebHostBuilder(
            hb => hb
                .ConfigureAppConfiguration((_, configuration) => ConfigureAppConfiguration(configuration))
                .ConfigureServices((context, collection) => ConfigureServices(collection, context.Configuration)));

        application.CreateClient();

        await using var innerScope = application.Services.CreateAsyncScope();
        var provider = innerScope.ServiceProvider;

        var producer = provider.GetRequiredService<IKafkaMessageProducer<int, string>>();

        var consumerConfig = new ConsumerConfig
        {
            GroupId = nameof(KafkaProducerTests),
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
        cts.CancelAfter(TimeSpan.FromSeconds(5 * testData.Messages.Length));

        // Assert
        producer.Should().BeOfType(configData.ExpectedProducerType);

        var consumedMessages = testData.Messages
            .Select(
                _ =>
                {
                    var result = consumer.Consume(cts.Token);
                    consumer.Commit(result);

                    return result;
                })
            .OrderBy(x => x.Offset.Value)
            .Select(x => x.Message)
            .ToArray();

        consumedMessages.Zip(testData.Messages)
            .Should()
            .AllSatisfy(
                tuple => tuple.First
                    .Should()
                    .BeEquivalentTo(
                        tuple.Second,
                        opt => opt
                            .Including(x => x.Key)
                            .Including(x => x.Value)));

        var outboxRepository = provider.GetRequiredService<IMessagePersistenceInternalRepository>();

        var query = SerializedMessageQuery.Build(builder => builder
            .WithPageSize(int.MaxValue)
            .WithName(KafkaOutboxMessageName.ForTopic(TopicName))
            .WithCursor(DateTimeOffset.MinValue)
            .WithState(MessageState.Completed));

        var outboxMessages = await outboxRepository
            .QueryAsync(query, default)
            .ToArrayAsync(default);

        if (configData.ShouldWriteOutboxMessages)
        {
            outboxMessages.Should().HaveCount(testData.Messages.Length);
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
            new(null, typeof(AlwaysOutboxMessageProducer<int, string>), true),
            new(OutboxStrategy.Always, typeof(AlwaysOutboxMessageProducer<int, string>), true),
            new(OutboxStrategy.Fallback, typeof(FallbackOutboxMessageProducer<int, string>), false),
        ];

        KafkaOutboxTestData[] data =
        [
            KafkaOutboxTestData.SingleMessage(1),
            KafkaOutboxTestData.SingleMessage(10),
            KafkaOutboxTestData.Many(1, 10),
            KafkaOutboxTestData.Many(5, 10),
            KafkaOutboxTestData.Many(10, 10),
        ];

        return data.SelectMany(_ => configs, static (data, config) => new object[] { data, config });
    }

    public async Task InitializeAsync()
    {
        await _kafkaFixture.CreateTopicsAsync(TopicName);
    }

    public async Task DisposeAsync()
    {
        const string truncateSql = """
        truncate table message_persistence.persisted_messages;
        """;

        await using var command = _databaseFixture.Connection.CreateCommand();
        command.CommandText = truncateSql;

        if (_databaseFixture.Connection.State is not ConnectionState.Open)
        {
            await _databaseFixture.Connection.OpenAsync();
        }

        await command.ExecuteNonQueryAsync();

        await _databaseFixture.ResetAsync();
    }
}