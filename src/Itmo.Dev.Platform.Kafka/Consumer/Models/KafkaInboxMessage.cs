using Confluent.Kafka;
using Itmo.Dev.Platform.Common.Models;
using Itmo.Dev.Platform.MessagePersistence;

namespace Itmo.Dev.Platform.Kafka.Consumer.Models;

internal class KafkaInboxMessage<TKey, TValue> : IKafkaInboxMessage<TKey, TValue>
{
    private readonly IMessage<Unit, KafkaConsumerMessage<TKey, TValue>> _message;

    public KafkaInboxMessage(IMessage<Unit, KafkaConsumerMessage<TKey, TValue>> message)
    {
        _message = message;
    }

    public TKey Key => _message.Value.Key;

    public TValue Value => _message.Value.Value;

    public DateTimeOffset Timestamp => _message.Value.Timestamp;

    public string Topic => _message.Value.Topic;

    public Partition Partition => _message.Value.Partition;

    public Offset Offset => _message.Value.Offset;

    public void SetSuccessResult()
    {
        _message.SetSuccessResult();
    }

    public void SetIgnoredResult()
    {
        _message.SetIgnoredResult();
    }

    public void SetFailedResult(Exception? exception = null)
    {
        _message.SetFailedResult(exception);
    }
}
