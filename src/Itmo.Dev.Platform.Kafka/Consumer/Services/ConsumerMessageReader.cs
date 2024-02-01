using Confluent.Kafka;
using Itmo.Dev.Platform.Kafka.Configuration;
using Itmo.Dev.Platform.Kafka.Consumer.Models;
using System.Threading.Channels;

namespace Itmo.Dev.Platform.Kafka.Consumer.Services;

internal class ConsumerMessageReader<TKey, TValue>
{
    private readonly PlatformKafkaOptions _kafkaOptions;
    private readonly KafkaConsumerOptions _consumerOptions;
    private readonly IDeserializer<TKey>? _keyDeserializer;
    private readonly IDeserializer<TValue>? _valueDeserializer;

    public ConsumerMessageReader(
        PlatformKafkaOptions kafkaOptions,
        KafkaConsumerOptions consumerOptions,
        IDeserializer<TKey>? keyDeserializer,
        IDeserializer<TValue>? valueDeserializer)
    {
        _kafkaOptions = kafkaOptions;
        _consumerOptions = consumerOptions;
        _keyDeserializer = keyDeserializer;
        _valueDeserializer = valueDeserializer;
    }

    public async Task ReadAsync(
        ChannelWriter<KafkaConsumerMessage<TKey, TValue>> writer,
        CancellationToken cancellationToken)
    {
        await Task.Yield();

        var consumerConfiguration = new ConsumerConfig
        {
            BootstrapServers = _kafkaOptions.Host,
            SecurityProtocol = _kafkaOptions.SecurityProtocol,
            SslCaPem = _kafkaOptions.SslCaPem,
            SaslMechanism = _kafkaOptions.SaslMechanism,
            SaslUsername = _kafkaOptions.SaslUsername,
            SaslPassword = _kafkaOptions.SaslPassword,

            GroupId = _consumerOptions.Group,
            GroupInstanceId = _consumerOptions.InstanceId,
            AutoOffsetReset = _consumerOptions.ReadLatest ? AutoOffsetReset.Latest : AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        };

        using IConsumer<TKey, TValue> consumer = new ConsumerBuilder<TKey, TValue>(consumerConfiguration)
            .SetKeyDeserializer(_keyDeserializer)
            .SetValueDeserializer(_valueDeserializer)
            .Build();

        consumer.Subscribe(_consumerOptions.Topic);

        try
        {
            while (cancellationToken.IsCancellationRequested is false)
            {
                ConsumeResult<TKey, TValue> result = consumer.Consume(cancellationToken);
                var message = new KafkaConsumerMessage<TKey, TValue>(consumer, result);

                await writer.WriteAsync(message, cancellationToken);
            }
        }
        finally
        {
            consumer.Close();
        }
    }
}