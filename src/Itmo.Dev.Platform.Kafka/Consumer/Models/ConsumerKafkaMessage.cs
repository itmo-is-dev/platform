using Confluent.Kafka;

namespace Itmo.Dev.Platform.Kafka.Consumer.Models;

public record ConsumerKafkaMessage<TKey, TValue>(
    TKey Key,
    TValue Value,
    DateTimeOffset Timestamp,
    string Topic,
    Partition Partition,
    Offset Offset);