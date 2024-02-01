namespace Itmo.Dev.Platform.Kafka.Producer;

public record KafkaProducerMessage<TKey, TValue>(TKey Key, TValue Value);