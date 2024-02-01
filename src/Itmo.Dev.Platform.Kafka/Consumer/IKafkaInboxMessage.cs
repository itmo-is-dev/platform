using Itmo.Dev.Platform.MessagePersistence;

namespace Itmo.Dev.Platform.Kafka.Consumer;

public interface IKafkaInboxMessage<out TKey, out TValue> : IKafkaConsumerMessage<TKey, TValue>
{
    void SetResult(MessageHandleResult result);
}