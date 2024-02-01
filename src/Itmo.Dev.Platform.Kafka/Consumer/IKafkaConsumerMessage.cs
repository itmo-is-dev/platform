using Confluent.Kafka;

namespace Itmo.Dev.Platform.Kafka.Consumer;

public interface IKafkaConsumerMessage<out TKey, out TValue>
{
    public TKey Key { get; }
    public TValue Value { get; }
    public DateTimeOffset Timestamp { get; }
    public string Topic { get; }
    public Partition Partition { get; }
    public Offset Offset { get; }
}