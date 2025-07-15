namespace Itmo.Dev.Platform.Kafka.Producer;

public record KafkaProducerMessage<TKey, TValue>(TKey Key, TValue Value);

public static class KafkaProducerMessage
{
    public static KafkaProducerMessage<TKey, TValue> Create<TKey, TValue>(TKey key, TValue value) => new(key, value);
}
