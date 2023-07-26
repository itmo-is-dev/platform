namespace Itmo.Dev.Platform.Kafka.Producer.Models;

public record ProducerKafkaMessage<TKey, TValue>(TKey Key, TValue Value);