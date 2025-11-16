using Confluent.Kafka;
using FluentAssertions;
using Itmo.Dev.Platform.Kafka.Consumer;
using Itmo.Dev.Platform.Kafka.Extensions;
using Itmo.Dev.Platform.Kafka.Producer;
using Itmo.Dev.Platform.Kafka.Tests.Extensions;
using Itmo.Dev.Platform.Kafka.Tests.Fixtures;
using Itmo.Dev.Platform.Kafka.Tests.Tools;
using Itmo.Dev.Platform.Kafka.Tools;
using Itmo.Dev.Platform.Testing.ApplicationFactories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Itmo.Dev.Platform.Kafka.Tests;

#pragma warning disable CA1506

[Collection(nameof(KafkaCollectionFixture))]
public class KafkaProducerTests : IAsyncLifetime
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
    public async Task ProduceAsync_ShouldWriteMessage(KafkaProducerMessage<int, string>[] messages)
    {
        // Arrange
        var collection = new ServiceCollection();

        var configuration = new ConfigurationManager();

        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            [nameof(KafkaProducerOptions.Topic)] = TopicName,
        });

        collection.AddPlatformKafka(builder => builder
            .ConfigureTestOptions(_kafkaFixture.Host)
            .AddProducer(b => b
                .WithKey<int>()
                .WithValue<string>()
                .WithConfiguration(configuration)
                .SerializeKeyWithNewtonsoft()
                .SerializeValueWithNewtonsoft()));

        collection.AddLogging();
        collection.AddSerilog();

        var provider = collection.BuildServiceProvider();

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
            new[]
            {
                new KafkaProducerMessage<int, string>(1, "aboba"),
            },
        ];

        yield return
        [
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
