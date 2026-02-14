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
using Itmo.Dev.Platform.MessagePersistence.Internal.Models;
using Itmo.Dev.Platform.MessagePersistence.Internal.Persistence;
using Itmo.Dev.Platform.MessagePersistence.Options;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Configuration;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Extensions;
using Itmo.Dev.Platform.Testing.ApplicationFactories;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable AccessToDisposedClosure

namespace Itmo.Dev.Platform.Kafka.Tests;

#pragma warning disable CA1506

[Collection(nameof(KafkaCollectionFixture))]
public class MessagePersistenceKafkaBufferingTests : IAsyncLifetime, IClassFixture<KafkaDatabaseFixture>
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
                        .WithMessage<TestPersistenceMessage<int, int>>()
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
            GroupId = TestConsumerGroup.GetName(),
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        using var consumer = new ConsumerBuilder<BufferedMessageKey, BufferedMessageValue>(consumerConfig)
            .SetKeyDeserializer(new NewtonsoftJsonValueSerializer<BufferedMessageKey>())
            .SetValueDeserializer(new NewtonsoftJsonValueSerializer<BufferedMessageValue>())
            .Build();

        consumer.Subscribe(TopicName);

        var persistedMessage = TestPersistenceMessage.Create(1, 1);

        await using var scope = application.Services.CreateAsyncScope();
        var persistedMessageConsumer = scope.ServiceProvider.GetRequiredService<IMessagePersistenceService>();

        var messagePersistenceRepository = scope.ServiceProvider
            .GetRequiredService<IMessagePersistenceInternalRepository>();

        // Act
        using var cts = new CancellationTokenSource();
        cts.CancelAfterDebug(TimeSpan.FromSeconds(15));

        var messageId = await persistedMessageConsumer
            .PersistInternalAsync(
                [persistedMessage],
                cts.Token)
            .SingleAsync(cts.Token);

        // Assert
        await testContext.Message.WaitAsync(cts.Token);

        var serializedMessage = await messagePersistenceRepository.PollMessageAsync(
            messageId,
            MessageState.Completed,
            cts.Token);

        var kafkaConsumeResult = await consumer
            .ConsumePersistedMessageAsync(messageId, cts.Token)
            .WrapCancellationWithMessage("Failed to consume message from buffer topic");

        serializedMessage.State.Should().Be(MessageState.Completed);
        serializedMessage.RetryCount.Should().Be(0);

        kafkaConsumeResult.Message.Key.Key.Should().Be(persistedMessage.Key.ToString());

        kafkaConsumeResult.Message.Value.Message
            .Should()
            .BeEquivalentTo(serializedMessage,
                options => options
                    .Excluding(message => message.State)
                    .Excluding(message => message.Headers));
    }

    [Fact]
    public async Task Consume_ShouldRetryMessageThroughBuffer_WhenKafkaBufferingConfigured()
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
                    "Handler": {
                        "{{nameof(MessagePersistenceHandlerOptions.RetryCount)}}": 2
                    }
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
                        .WithMessage<TestPersistenceMessage<int, int>>()
                        .HandleBy<FailingTestPersistenceHandler<int, int>>()
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
            GroupId = TestConsumerGroup.GetName(),
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        using var consumer = new ConsumerBuilder<BufferedMessageKey, BufferedMessageValue>(consumerConfig)
            .SetKeyDeserializer(new NewtonsoftJsonValueSerializer<BufferedMessageKey>())
            .SetValueDeserializer(new NewtonsoftJsonValueSerializer<BufferedMessageValue>())
            .Build();

        consumer.Subscribe(TopicName);

        var persistedMessage = TestPersistenceMessage.Create(1, 1);

        await using var scope = application.Services.CreateAsyncScope();
        var persistedMessageConsumer = scope.ServiceProvider.GetRequiredService<IMessagePersistenceService>();

        var messagePersistenceRepository = scope.ServiceProvider
            .GetRequiredService<IMessagePersistenceInternalRepository>();

        // Act
        using var cts = new CancellationTokenSource();
        cts.CancelAfterDebug(TimeSpan.FromSeconds(15));

        var messageId = await persistedMessageConsumer
            .PersistInternalAsync(
                [persistedMessage],
                cts.Token)
            .SingleAsync(cts.Token);

        // Assert
        await testContext.Message
            .WaitAsync(cts.Token)
            .WrapCancellationWithMessage("Failed to wait message completion");

        var serializedMessage = await messagePersistenceRepository.PollMessageAsync(
            messageId,
            MessageState.Completed,
            cts.Token);

        var firstMessageConsumeResult = await consumer
            .ConsumePersistedMessageAsync(messageId, cts.Token)
            .WrapCancellationWithMessage("Failed to consume initial message");

        var secondMessageConsumeResult = await consumer
            .ConsumePersistedMessageAsync(messageId, cts.Token)
            .WrapCancellationWithMessage("Failed to consume retry message");

        firstMessageConsumeResult.Message.Key.Key.Should().Be(persistedMessage.Key.ToString());
        secondMessageConsumeResult.Message.Key.Key.Should().Be(persistedMessage.Key.ToString());

        firstMessageConsumeResult.Message.Value.Message
            .Should()
            .BeEquivalentTo(serializedMessage,
                options => options
                    .Excluding(message => message.State)
                    .Excluding(message => message.RetryCount)
                    .Excluding(message => message.Headers));

        firstMessageConsumeResult.Message.Value.Message.RetryCount.Should().Be(0);

        secondMessageConsumeResult.Message.Value.Message
            .Should()
            .BeEquivalentTo(serializedMessage,
                options => options
                    .Excluding(message => message.State)
                    .Excluding(message => message.Headers));
    }

    [Fact]
    public async Task ConsumeAsync_ShouldProduceSingleKafkaMessage_WhenKafkaBufferingConfiguredInBlockingMode()
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
                    "Handler": {
                        "{{nameof(MessagePersistenceHandlerOptions.RetryCount)}}": 2
                    }
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
                            .WithConsumerConfiguration(configuration.GetSection("Consumer"))
                            .WithFailureBlockingBehaviour()))
                    .AddMessage(message => message
                        .Called(nameof(MessagePersistenceKafkaBufferingTests))
                        .WithConfiguration("MessagePersistence:Handler")
                        .WithMessage<TestPersistenceMessage<int, int>>()
                        .HandleBy<FailingTestPersistenceHandler<int, int>>()
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
            GroupId = TestConsumerGroup.GetName(),
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        using var consumer = new ConsumerBuilder<BufferedMessageKey, BufferedMessageValue>(consumerConfig)
            .SetKeyDeserializer(new NewtonsoftJsonValueSerializer<BufferedMessageKey>())
            .SetValueDeserializer(new NewtonsoftJsonValueSerializer<BufferedMessageValue>())
            .Build();

        consumer.Subscribe(TopicName);

        var persistedMessage = TestPersistenceMessage.Create(1, 1);

        await using var scope = application.Services.CreateAsyncScope();
        var persistedMessageConsumer = scope.ServiceProvider.GetRequiredService<IMessagePersistenceService>();

        var messagePersistenceRepository = scope.ServiceProvider
            .GetRequiredService<IMessagePersistenceInternalRepository>();

        // Act

        using var cts = new CancellationTokenSource();
        cts.CancelAfterDebug(TimeSpan.FromSeconds(15));

        var messageId = await persistedMessageConsumer
            .PersistInternalAsync(
                [persistedMessage],
                cts.Token)
            .SingleAsync(cts.Token);

        // Assert

        await testContext.Message
            .WaitAsync(cts.Token)
            .WrapCancellationWithMessage("Failed to wait message completion");

        var serializedMessage = await messagePersistenceRepository.PollMessageAsync(
            messageId,
            MessageState.Completed,
            cts.Token);

        var firstMessageConsumeResult = await consumer
            .ConsumePersistedMessageAsync(messageId, cts.Token)
            .WrapCancellationWithMessage("Failed to consume initial message");

        using var secondMessageCts = new CancellationTokenSource();
        secondMessageCts.CancelAfterDebug(TimeSpan.FromMilliseconds(500));

        var secondMessageConsume = () => consumer.ConsumeWithCancellationMessage(
            "Failed to consume retry message",
            secondMessageCts.Token);

        secondMessageConsume.Should().Throw<OperationCanceledException>();

        serializedMessage.State.Should().Be(MessageState.Completed);

        firstMessageConsumeResult.Message.Key.Key.Should().Be(persistedMessage.Key.ToString());

        firstMessageConsumeResult.Message.Value.Message
            .Should()
            .BeEquivalentTo(serializedMessage,
                options => options
                    .Excluding(message => message.State)
                    .Excluding(message => message.RetryCount)
                    .Excluding(message => message.Headers));

        firstMessageConsumeResult.Message.Value.Message.RetryCount.Should().Be(0);
    }

    public async Task InitializeAsync()
    {
        await _kafkaFixture.CreateTopicsAsync(TopicName);
    }

    public async Task DisposeAsync()
    {
        await _kafkaFixture.ClearTopicsAsync(TopicName);
        await Task.Delay(TimeSpan.FromMilliseconds(200));
    }
}

static file class MessagePersistenceRepositoryExtensions
{
    public static async Task<PersistedMessageModel> PollMessageAsync(
        this IMessagePersistenceInternalRepository repository,
        long messageId,
        MessageState state,
        CancellationToken cancellationToken)
    {
        while (cancellationToken.IsCancellationRequested is false)
        {
            var message = await repository
                .QueryAsync(
                    PersistedMessageQuery.Build(builder => builder
                        .WithPageSize(1)
                        .WithId(messageId)
                        .WithState(state)),
                    cancellationToken)
                .SingleOrDefaultAsync(cancellationToken);

            if (message is not null)
                return message;

            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
        }

        throw new OperationCanceledException(cancellationToken);
    }
}

static file class KafkaConsumerExtensions
{
    public static async Task<ConsumeResult<BufferedMessageKey, BufferedMessageValue>> ConsumePersistedMessageAsync(
        this IConsumer<BufferedMessageKey, BufferedMessageValue> consumer,
        long messageId,
        CancellationToken cancellationToken)
    {
        while (cancellationToken.IsCancellationRequested is false)
        {
            var result = consumer.Consume(cancellationToken);

            if (result.Message.Value.Message.Id == messageId)
                return result;

            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
        }

        throw new OperationCanceledException(cancellationToken);
    }
}
