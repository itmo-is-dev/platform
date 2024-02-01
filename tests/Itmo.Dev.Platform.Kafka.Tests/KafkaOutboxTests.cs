using Confluent.Kafka;
using FluentAssertions;
using Itmo.Dev.Platform.Kafka.Extensions;
using Itmo.Dev.Platform.Kafka.Producer;
using Itmo.Dev.Platform.Kafka.Tests.Extensions;
using Itmo.Dev.Platform.Kafka.Tests.Fixtures;
using Itmo.Dev.Platform.Kafka.Tools;
using Itmo.Dev.Platform.MessagePersistence.Configuration;
using Itmo.Dev.Platform.MessagePersistence.Extensions;
using Itmo.Dev.Platform.Postgres.Extensions;
using Itmo.Dev.Platform.Postgres.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
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
    public async Task ProduceAsync_ShouldWriteMessage(int bufferSize, KafkaProducerMessage<int, string>[] messages)
    {
        // Arrange
        await using var fixtureScope = _databaseFixture.Scope;

        void ConfigureAppConfiguration(IConfigurationBuilder configuration)
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MessagePersistence:SchemaName"] = "message_persistence",
                [$"Producer:{nameof(KafkaProducerOptions.Topic)}"] = TopicName,
                [$"Producer:Outbox:{nameof(MessagePersistenceHandlerOptions.BatchSize)}"] = bufferSize.ToString(),
                [$"Producer:Outbox:{nameof(MessagePersistenceHandlerOptions.PollingDelay)}"] = "00:00:00.500",
            });
        }

        void ConfigureServices(IServiceCollection collection, IConfiguration configuration)
        {
            collection.AddPlatformMessagePersistence(builder => builder
                .ConfigurePersistence(configuration.GetSection("MessagePersistence")));

            collection.AddKafka(builder => builder
                .ConfigureTestOptions(_kafkaFixture.Host)
                .AddProducer(b => b
                    .WithKey<int>()
                    .WithValue<string>()
                    .WithConfiguration(configuration.GetSection("Producer"))
                    .SerializeKeyWithNewtonsoft()
                    .SerializeValueWithNewtonsoft()
                    .WithOutbox()));

            var connectionString = fixtureScope.ServiceProvider.GetRequiredService<PostgresConnectionString>();

            collection.AddPlatformPostgres(_ => { });
            collection.RemoveAll<PostgresConnectionString>();
            collection.AddSingleton(connectionString);

            collection.AddLogging(x => x.AddSerilog());
            collection.AddOptions();
        }

        await using var application = new WebApplicationFactory<Program>().WithWebHostBuilder(hb => hb
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

        await producer.ProduceAsync(messages.ToAsyncEnumerable(), default);

        // Act

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(5 * messages.Length));

        // Assert
        var consumedMessages = messages
            .Select(_ =>
            {
                var result = consumer.Consume(cts.Token);
                consumer.Commit(result);

                return result;
            })
            .OrderBy(x => x.Offset.Value)
            .Select(x => x.Message)
            .ToArray();

        consumedMessages.Zip(messages)
            .Should()
            .AllSatisfy(tuple => tuple.First
                .Should()
                .BeEquivalentTo(
                    tuple.Second,
                    opt => opt
                        .Including(x => x.Key)
                        .Including(x => x.Value)));

        // Dispose
        consumer.Close();
    }

    public static IEnumerable<object[]> GetMessages()
    {
        yield return
        [
            1,
            new[]
            {
                new KafkaProducerMessage<int, string>(1, "aboba"),
            },
        ];


        yield return
        [
            10,
            new[]
            {
                new KafkaProducerMessage<int, string>(1, "aboba"),
            },
        ];

        yield return
        [
            1,
            Enumerable
                .Range(0, 10)
                .Select(i => new KafkaProducerMessage<int, string>(i, i.ToString()))
                .ToArray(),
        ];

        yield return
        [
            5,
            Enumerable
                .Range(0, 10)
                .Select(i => new KafkaProducerMessage<int, string>(i, i.ToString()))
                .ToArray(),
        ];

        yield return
        [
            5,
            Enumerable
                .Range(0, 10)
                .Select(i => new KafkaProducerMessage<int, string>(i, i.ToString()))
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