using Itmo.Dev.Platform.Kafka.Producer;

namespace Itmo.Dev.Platform.Kafka.Extensions;

public static class ProducerExtensions
{
    public static Task ProduceAsync<TKey, TValue>(
        this IKafkaMessageProducer<TKey, TValue> producer,
        KafkaProducerMessage<TKey, TValue> message,
        CancellationToken cancellationToken)
    {
        var enumerable = new[] { message }.ToAsyncEnumerable();
        return producer.ProduceAsync(enumerable, cancellationToken);
    }
}