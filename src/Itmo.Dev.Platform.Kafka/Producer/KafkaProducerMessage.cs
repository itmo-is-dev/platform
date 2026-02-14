namespace Itmo.Dev.Platform.Kafka.Producer;

public record KafkaProducerMessage<TKey, TValue>(
    TKey Key,
    TValue Value,
    IDictionary<string, string>? Headers = null);

public static class KafkaProducerMessage
{
    public static KafkaProducerMessage<TKey, TValue> Create<TKey, TValue>(TKey key, TValue value) => new(key, value);

    public static KafkaProducerMessage<TKey, TValue> Create<TKey, TValue>(
        TKey key,
        TValue value,
        IDictionary<string, string> headers)
    {
        return new KafkaProducerMessage<TKey, TValue>(key, value, headers);
    }
}
