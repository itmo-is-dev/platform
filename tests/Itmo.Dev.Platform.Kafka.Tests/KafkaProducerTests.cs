using Confluent.Kafka;
using FluentAssertions;
using Itmo.Dev.Platform.Kafka.Extensions;
using Itmo.Dev.Platform.Kafka.Producer;
using Itmo.Dev.Platform.Kafka.Producer.Models;
using Itmo.Dev.Platform.Kafka.Tests.Extensions;
using Itmo.Dev.Platform.Kafka.Tests.Fixtures;
using Itmo.Dev.Platform.Kafka.Tools;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Itmo.Dev.Platform.Kafka.Tests;

public class KafkaProducerTests : IClassFixture<KafkaFixture>, IAsyncLifetime
{
    private const string TopicName = $"{nameof(KafkaProducerTests)}_topic";

    private readonly KafkaFixture _kafkaFixture;

    public KafkaProducerTests(KafkaFixture kafkaFixture, ITestOutputHelper output)
    {
        _kafkaFixture = kafkaFixture;

        Log.Logger = new LoggerConfiguration()
            .WriteTo.TestOutput(output)
            .CreateLogger();
    }

    [Theory]
    [MemberData(nameof(GetMessages))]
    public async Task ProduceAsync_ShouldWriteMessage(ProducerKafkaMessage<int, string>[] messages)
    {
        // Arrange
        var collection = new ServiceCollection();

        var configuration = new Configuration(_kafkaFixture.Host);
        collection.AddObjectAsOptions(configuration);

        collection.AddKafkaProducer<int, string>(builder => builder
            .SerializeKeyWithNewtonsoft()
            .SerializeValueWithNewtonsoft()
            .UseConfiguration<Configuration>());

        collection.AddLogging();
        collection.AddSerilog();

        var provider = collection.BuildServiceProvider();

        var producer = provider.GetRequiredService<IKafkaMessageProducer<int, string>>();

        var consumerConfig = new ConsumerConfig
        {
            GroupId = nameof(KafkaProducerTests),
            BootstrapServers = configuration.Host,
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        using var consumer = new ConsumerBuilder<int, string>(consumerConfig)
            .SetKeyDeserializer(new NewtonsoftJsonValueSerializer<int>())
            .SetValueDeserializer(new NewtonsoftJsonValueSerializer<string>())
            .Build();

        consumer.Subscribe(TopicName);

        // Act
        await producer.ProduceAsync(messages.ToAsyncEnumerable(), default);

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
    }

    public static IEnumerable<object[]> GetMessages()
    {
        yield return new object[]
        {
            new[]
            {
                new ProducerKafkaMessage<int, string>(1, "aboba"),
            },
        };

        yield return new object[]
        {
            Enumerable
                .Range(0, 10)
                .Select(i => new ProducerKafkaMessage<int, string>(i, i.ToString()))
                .ToArray(),
        };
    }

    public async Task InitializeAsync()
    {
        await _kafkaFixture.CreateTopicsAsync(TopicName);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public class Configuration : IKafkaProducerConfiguration
    {
        public Configuration(string host)
        {
            Host = host;
        }

        public string Host { get; }

        public string Topic => TopicName;

        public IKafkaProducerConfiguration WithHost(string host)
        {
            throw new NotImplementedException();
        }
    }
}