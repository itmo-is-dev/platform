using Itmo.Dev.Platform.Kafka.Producer;
using Itmo.Dev.Platform.Kafka.Producer.Models;

namespace Itmo.Dev.Platform.Kafka.Extensions;

public static class ProducerExtensions
{
    public static Task ProduceAsync<TKey, TValue>(
        this IKafkaMessageProducer<TKey, TValue> producer,
        ProducerKafkaMessage<TKey, TValue> message,
        CancellationToken cancellationToken)
    {
        var enumerable = new[] { message }.ToAsyncEnumerable();
        return producer.ProduceAsync(enumerable, cancellationToken);
    }
}