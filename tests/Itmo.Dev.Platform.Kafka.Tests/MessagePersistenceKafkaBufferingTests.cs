using Confluent.Kafka;
using FluentAssertions;
using Itmo.Dev.Platform.Kafka.Consumer;
using Itmo.Dev.Platform.Kafka.Extensions;
using Itmo.Dev.Platform.Kafka.MessagePersistence.Models;
using Itmo.Dev.Platform.Kafka.Producer;
using Itmo.Dev.Platform.Kafka.Tests.Extensions;
using Itmo.Dev.Platform.Kafka.Tests.Fixtures;
using Itmo.Dev.Platform.Kafka.Tests.MessagePersistenceBuffering;
using Itmo.Dev.Platform.Kafka.Tests.Tools;
using Itmo.Dev.Platform.Kafka.Tools;
using Itmo.Dev.Platform.MessagePersistence;
using Itmo.Dev.Platform.MessagePersistence.Models;
using Itmo.Dev.Platform.MessagePersistence.Options;
using Itmo.Dev.Platform.MessagePersistence.Persistence;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Configuration;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Extensions;
using Itmo.Dev.Platform.Testing;
using Itmo.Dev.Platform.Testing.ApplicationFactories;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Xunit;
using Xunit.Abstractions;

namespace Itmo.Dev.Platform.Kafka.Tests;

#pragma warning disable CA1506

[Collection(nameof(KafkaCollectionFixture))]
public class MessagePersistenceKafkaBufferingTests : IAsyncInitializeLifetime, IClassFixture<KafkaDatabaseFixture>
{
    private const string TopicName = $"{nameof(MessagePersistenceKafkaBufferingTests)}_topic";

    private readonly KafkaFixture _kafkaFixture;
    private readonly KafkaDatabaseFixture _databaseFixture;

    public MessagePersistenceKafkaBufferingTests(
        KafkaFixture kafkaFixture,
        ITestOutputHelper output,
        KafkaDatabaseFixture databaseFixture)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.TestOutput(output)
            .MinimumLevel.Override("Npgsql", LogEventLevel.Warning)
            .CreateLogger();

        _kafkaFixture = kafkaFixture;
        _databaseFixture = databaseFixture;
    }

    [Fact]
    public async Task Consume_ShouldHandleMessageThroughBuffer_WhenKafkaBufferingConfigured()
    {
        // Arrange
        await using var fixtureScope = _databaseFixture.Scope;
        var testContext = new TestContext<int, int>();

        await using var application = new PlatformWebApplicationBuilder<Program>()
            .AddConfigurationJson(
                $$"""
            {
                "MessagePersistence": {
                    "{{nameof(MessagePersistencePostgresOptions.SchemaName)}}": "message_persistence",
                    "Publisher": {
                        "Default": {
                            "{{nameof(MessagePersistencePublisherOptions.BatchSize)}}": 1,
                            "{{nameof(MessagePersistencePublisherOptions.PollingDelay)}}": "00:00:00.500"
                        }
                    },
                    "Handler": {}
                },
                "Producer": {
                    "{{nameof(KafkaProducerOptions.Topic)}}": "{{TopicName}}"
                },
                "Consumer": {
                    "{{nameof(KafkaConsumerOptions.Topic)}}": "{{TopicName}}",
                    "{{nameof(KafkaConsumerOptions.Group)}}": "{{nameof(MessagePersistenceKafkaBufferingTests)}}",
                    "{{nameof(KafkaConsumerOptions.InstanceId)}}": "{{nameof(MessagePersistenceKafkaBufferingTests)}}",
                    "{{nameof(KafkaConsumerOptions.BufferWaitLimit)}}": "00:00:00.200",
                    "{{nameof(KafkaConsumerOptions.BufferSize)}}": 1
                }
            }
            """)
            .ConfigureServices(x => x.AddSingleton(testContext))
            .ConfigureServices((collection, configuration) => collection
                .AddPlatformMessagePersistence(builder => builder
                    .WithDefaultPublisherOptions("MessagePersistence:Publisher:Default")
                    .UsePostgresPersistence(configurator => configurator.ConfigureOptions("MessagePersistence"))
                    .AddBufferingGroup(group => group
                        .Called(nameof(MessagePersistenceKafkaBufferingTests))
                        .WithPublisherConfiguration("MessagePersistence:Publisher:Default")
                        .AddKafkaBufferingStep(kafka => kafka
                            .WithProducerConfiguration(configuration.GetSection("Producer"))
                            .WithConsumerConfiguration(configuration.GetSection("Consumer"))))
                    .AddMessage(message => message
                        .Called(nameof(MessagePersistenceKafkaBufferingTests))
                        .WithConfiguration("MessagePersistence:Handler")
                        .WithKey<int>()
                        .WithValue<int>()
                        .HandleBy<TestPersistenceHandler<int, int>>()
                        .WithBufferingGroup(nameof(MessagePersistenceKafkaBufferingTests))))
            )
            .ConfigureServices(collection => collection
                .AddPlatformKafka(builder => builder.ConfigureTestOptions(_kafkaFixture.Host)))
            .ConfigureServices(collection => _databaseFixture.AddPlatformPersistence(collection))
            .Build();

        application.CreateClient();

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _kafkaFixture.Host,
            GroupId = nameof(MessagePersistenceKafkaBufferingTests) + "_validate",
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        using var consumer = new ConsumerBuilder<BufferedMessageKey, BufferedMessageValue>(consumerConfig)
            .SetKeyDeserializer(new NewtonsoftJsonValueSerializer<BufferedMessageKey>())
            .SetValueDeserializer(new NewtonsoftJsonValueSerializer<BufferedMessageValue>())
            .Build();

        consumer.Subscribe(TopicName);

        var persistedMessage = PersistedMessage.Create(1, 1);

        await using var scope = application.Services.CreateAsyncScope();
        var persistedMessageConsumer = scope.ServiceProvider.GetRequiredService<IMessagePersistenceConsumer>();

        var messagePersistenceRepository = scope.ServiceProvider
            .GetRequiredService<IMessagePersistenceInternalRepository>();

        // Act
        using var cts = new CancellationTokenSource();
        cts.CancelAfterDebug(TimeSpan.FromSeconds(15));

        await persistedMessageConsumer.ConsumeAsync(
            nameof(MessagePersistenceKafkaBufferingTests),
            [persistedMessage],
            cts.Token);

        // Assert
        await testContext.Message.WaitAsync(cts.Token);

        var kafkaConsumeResult = consumer.Consume(cts.Token);

        var serializedMessage = await messagePersistenceRepository
            .QueryAsync(SerializedMessageQuery.Build(b => b.WithPageSize(1)), cts.Token)
            .SingleAsync(cts.Token);

        serializedMessage.State.Should().Be(MessageState.Completed);

        kafkaConsumeResult.Message.Key.Key.Should().Be(persistedMessage.Key.ToString());

        kafkaConsumeResult.Message.Value.Message
            .Should()
            .BeEquivalentTo(serializedMessage, options => options.Excluding(message => message.State));
    }

    public async Task InitializeAsync()
    {
        await _kafkaFixture.CreateTopicsAsync(TopicName);
    }
}
