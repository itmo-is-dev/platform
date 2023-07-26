using Confluent.Kafka;
using Itmo.Dev.Platform.Kafka.Producer.Models;
using Itmo.Dev.Platform.Kafka.Tools;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Kafka.Producer.Services;

internal class KafkaMessageProducer<TKey, TValue> : IKafkaMessageProducer<TKey, TValue>, IDisposable
{
    private readonly ILogger<KafkaMessageProducer<TKey, TValue>> _logger;
    private readonly IOptionsSnapshot<IKafkaProducerConfiguration> _configuration;
    private readonly IProducer<TKey, TValue> _producer;

    public KafkaMessageProducer(
        IServiceProvider provider,
        ILogger<KafkaMessageProducer<TKey, TValue>> logger,
        KeyValueQualifiedService<TKey, TValue, IOptionsSnapshot<IKafkaProducerConfiguration>> configurationResolver,
        ISerializer<TKey> keySerializer,
        ISerializer<TValue> valueSerializer)
    {
        _logger = logger;
        _configuration = configurationResolver.Resolve(provider);

        var config = new ProducerConfig
        {
            BootstrapServers = _configuration.Value.Host,
            MessageMaxBytes = _configuration.Value.MessageMaxBytes,
        };

        _producer = new ProducerBuilder<TKey, TValue>(config)
            .SetKeySerializer(keySerializer)
            .SetValueSerializer(valueSerializer)
            .Build();
    }

    public async Task ProduceAsync(
        IAsyncEnumerable<ProducerKafkaMessage<TKey, TValue>> messages,
        CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var producerKafkaMessage in messages.WithCancellation(cancellationToken))
            {
                var message = new Message<TKey, TValue>
                {
                    Key = producerKafkaMessage.Key,
                    Value = producerKafkaMessage.Value,
                };

                await _producer.ProduceAsync(_configuration.Value.Topic, message, cancellationToken);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error producing in topic {Topic}", _configuration.Value.Topic);
            throw;
        }
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}