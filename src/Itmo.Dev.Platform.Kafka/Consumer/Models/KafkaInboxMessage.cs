using Confluent.Kafka;
using Itmo.Dev.Platform.Kafka.Consumer.Inbox;
using Itmo.Dev.Platform.MessagePersistence;

namespace Itmo.Dev.Platform.Kafka.Consumer.Models;

internal class KafkaInboxMessage<TKey, TValue> : IKafkaInboxMessage<TKey, TValue>
{
    private readonly IPersistedMessageReference<InboxPersistedMessage<TKey, TValue>> _message;

    public KafkaInboxMessage(IPersistedMessageReference<InboxPersistedMessage<TKey, TValue>> message)
    {
        _message = message;
    }

    public TKey Key => _message.Message.Key;

    public TValue Value => _message.Message.Value;

    public DateTimeOffset Timestamp => _message.Message.Timestamp;

    public string Topic => _message.Message.Topic;

    public Partition Partition => _message.Message.Partition;

    public Offset Offset => _message.Message.Offset;

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
