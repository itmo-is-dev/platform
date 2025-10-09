namespace Itmo.Dev.Platform.MessagePersistence;

public interface IMessage<out TKey, out TValue>
{
    DateTimeOffset CreatedAt { get; }

    TKey Key { get; }

    TValue Value { get; }

    void SetSuccessResult();

    void SetIgnoredResult();

    void SetFailedResult(Exception? exception = null);
}
