namespace Itmo.Dev.Platform.MessagePersistence.Models;

internal class Message<TKey, TValue> : IMessage<TKey, TValue>
{
    public Message(long id, DateTimeOffset createdAt, TKey key, TValue value, MessageHandleResult result)
    {
        Id = id;
        CreatedAt = createdAt;
        Key = key;
        Value = value;
        Result = result;
    }

    public long Id { get; }

    public DateTimeOffset CreatedAt { get; }

    public TKey Key { get; }

    public TValue Value { get; }

    public MessageHandleResult Result { get; private set; }

    public void SetSuccessResult()
    {
        Result = new MessageHandleResult.Success();
    }

    public void SetIgnoredResult()
    {
        Result = new MessageHandleResult.Ignored();
    }

    public void SetFailedResult(Exception? exception = null)
    {
        Result = new MessageHandleResult.Failure(exception);
    }
}
