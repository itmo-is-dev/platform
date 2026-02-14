namespace Itmo.Dev.Platform.Kafka.Consumer;

public interface IKafkaInboxMessage<out TKey, out TValue> : IKafkaConsumerMessage<TKey, TValue>
{
    void SetSuccessResult();

    void SetIgnoredResult();

    void SetFailedResult(Exception? exception = null);
}