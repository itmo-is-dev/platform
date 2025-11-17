using Confluent.Kafka;
using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.Kafka.Configuration;
using Itmo.Dev.Platform.Kafka.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;

namespace Itmo.Dev.Platform.Kafka.Producer.Services;

internal class KafkaMessageProducer<TKey, TValue> : IKafkaMessageProducer<TKey, TValue>, IDisposable
{
    private readonly ILogger<KafkaMessageProducer<TKey, TValue>> _logger;
    private readonly IProducer<TKey, TValue> _producer;
    private readonly KafkaProducerOptions _options;

    public KafkaMessageProducer(
        string topicName,
        IServiceProvider provider,
        ILogger<KafkaMessageProducer<TKey, TValue>> logger)
    {
        _logger = logger;

        var consumerOptionsMonitor = provider.GetRequiredService<IOptionsMonitor<KafkaProducerOptions>>();
        var kafkaOptionsMonitor = provider.GetRequiredService<IOptionsMonitor<PlatformKafkaOptions>>();

        _options = consumerOptionsMonitor.Get(topicName);
        var kafkaOptions = kafkaOptionsMonitor.CurrentValue;

        var keySerializer = provider.GetRequiredKeyedService<ISerializer<TKey>>(topicName);
        var valueSerializer = provider.GetRequiredKeyedService<ISerializer<TValue>>(topicName);

        var config = new ProducerConfig
        {
            BootstrapServers = kafkaOptions.Host,
            SecurityProtocol = kafkaOptions.SecurityProtocol,
            SslCaPem = kafkaOptions.SslCaPem,
            SaslMechanism = kafkaOptions.SaslMechanism,
            SaslUsername = kafkaOptions.SaslUsername,
            SaslPassword = kafkaOptions.SaslPassword,

            MessageMaxBytes = _options.MessageMaxBytes,
        };

        _producer = new ProducerBuilder<TKey, TValue>(config)
            .SetKeySerializer(keySerializer)
            .SetValueSerializer(valueSerializer)
            .Build();
    }

    public async Task ProduceAsync(
        IAsyncEnumerable<KafkaProducerMessage<TKey, TValue>> messages,
        CancellationToken cancellationToken)
    {
        using var activity = PlatformKafkaActivitySource.Value
            .StartActivity(
                name: "Kafka",
                ActivityKind.Producer,
                parentContext: default)
            .WithDisplayName($"[produce] {_options.Topic}");

        Header[] defaultHeaders = GetDefaultHeaders().ToArray();

        try
        {
            await foreach (var producerKafkaMessage in messages.WithCancellation(cancellationToken))
            {
                var headers = defaultHeaders
                    .Concat(GetMessageHeaders(producerKafkaMessage))
                    .DistinctBy(x => x.Key);

                var message = new Message<TKey, TValue>
                {
                    Key = producerKafkaMessage.Key,
                    Value = producerKafkaMessage.Value,
                    Headers = [..headers],
                };

                await _producer.ProduceAsync(_options.Topic, message, cancellationToken);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error producing in topic {Topic}", _options.Topic);
            throw;
        }
    }

    public void Dispose()
    {
        _producer.Dispose();
    }

    private static IEnumerable<Header> GetDefaultHeaders()
    {
        if (Activity.Current is { Id: { } traceId })
        {
            yield return new Header(PlatformKafkaConstants.Tracing.TraceParentHeader, Encoding.UTF8.GetBytes(traceId));
        }
    }

    private static IEnumerable<Header> GetMessageHeaders(KafkaProducerMessage<TKey, TValue> message)
    {
        if (message.Headers is not null)
        {
            foreach (var (key, value) in message.Headers)
            {
                yield return new Header(key, Encoding.UTF8.GetBytes(value));
            }
        }
    }
}
