using Confluent.Kafka;
using FluentAssertions;
using Itmo.Dev.Platform.Kafka.Consumer;
using Itmo.Dev.Platform.Kafka.Consumer.Models;
using Itmo.Dev.Platform.Kafka.Extensions;
using Itmo.Dev.Platform.Kafka.Tests.Extensions;
using Itmo.Dev.Platform.Kafka.Tests.Fixtures;
using Itmo.Dev.Platform.Kafka.Tests.Tools;
using Itmo.Dev.Platform.Kafka.Tools;
using Itmo.Dev.Platform.Testing.ApplicationFactories;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Itmo.Dev.Platform.Kafka.Tests;

[Collection(nameof(KafkaCollectionFixture))]
public class BatchingKafkaConsumerTests : IAsyncLifetime
{
    private const string TopicName = $"{nameof(BatchingKafkaConsumerTests)}_topic";

    private readonly KafkaFixture _kafkaFixture;
    private readonly List<ConsumerKafkaMessage<int, string>> _messages;
    private readonly CollectionConsumerHandler<int, string> _handler;

    private ApplicationFactory<int, string> _applicationFactory = null!;

    public BatchingKafkaConsumerTests(KafkaFixture kafkaFixture, ITestOutputHelper output)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.TestOutput(output)
            .CreateLogger();

        _kafkaFixture = kafkaFixture;

        _messages = new List<ConsumerKafkaMessage<int, string>>();
        _handler = new CollectionConsumerHandler<int, string>(_messages);
    }

    [Theory]
    [MemberData(nameof(GetMessages))]
    public async Task Consume_ShouldConsume_WhenMessageProduced(Message<int, string>[] messages)
    {
        // Arrange
        var config = new ProducerConfig
        {
            BootstrapServers = _kafkaFixture.Host,
            MessageMaxBytes = 1_000_000,
        };

        var producer = new ProducerBuilder<int, string>(config)
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

        while (_messages.Count != messages.Length && cts.IsCancellationRequested is false)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100), default);
        }

        Log.Information("Expected count = {Count}", messages.Length);
        Log.Information("Actual count = {Count}", _messages.Count);

        // Assert
        _messages.Zip(messages)
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
                new Message<int, string>
                {
                    Key = 1,
                    Value = "aboba",
                },
            },
        };

        yield return new object[]
        {
            Enumerable
                .Range(0, 10)
                .Select(i => new Message<int, string> { Key = i, Value = i.ToString() })
                .ToArray(),
        };
    }

    public async Task InitializeAsync()
    {
        await _kafkaFixture.CreateTopicsAsync(TopicName);

        _applicationFactory = new ApplicationFactory<int, string>(_kafkaFixture, _handler);
        _ = _applicationFactory.Server;
    }

    public async Task DisposeAsync()
    {
        await _applicationFactory.DisposeAsync();
    }

    public class ApplicationFactory<TKey, TValue> : ConfigurableWebApplicationFactory<KafkaEmptyStartup>
    {
        private readonly KafkaFixture _kafkaFixture;
        private readonly CollectionConsumerHandler<TKey, TValue> _handler;

        public ApplicationFactory(
            KafkaFixture kafkaFixture,
            CollectionConsumerHandler<TKey, TValue> handler)
        {
            _kafkaFixture = kafkaFixture;
            _handler = handler;
        }

        protected override void ConfigureServices(IServiceCollection collection)
        {
            collection.AddScoped(_ => _handler);

            collection.AddKafkaConsumer<TKey, TValue>(builder => builder
                .HandleWith<CollectionConsumerHandler<TKey, TValue>>()
                .DeserializeKeyWithNewtonsoft()
                .DeserializeValueWithNewtonsoft()
                .UseConfiguration<Configuration>());

            collection.AddSerilog();

            var configuration = new Configuration(_kafkaFixture.Host);

            collection.AddOptions();
            collection.AddObjectAsOptions(configuration);
        }
    }

    public class Configuration : IKafkaConsumerConfiguration
    {
        public Configuration(string host)
        {
            Host = host;
        }

        public bool IsDisabled => false;

        public TimeSpan DisabledConsumerTimeout => TimeSpan.FromSeconds(10);

        public string Host { get; }

        public string Topic => TopicName;

        public string Group => nameof(KafkaConsumerTests);

        public int ParallelismDegree => 1;

        public int BufferSize => 2;

        public TimeSpan BufferWaitLimit => TimeSpan.FromMilliseconds(200);

        public bool ReadLatest => false;

        public SecurityProtocol SecurityProtocol => SecurityProtocol.Plaintext;

        public IKafkaConsumerConfiguration WithHost(string host)
        {
            throw new NotImplementedException();
        }

        public IKafkaConsumerConfiguration WithGroup(string group)
        {
            throw new NotImplementedException();
        }
    }
}