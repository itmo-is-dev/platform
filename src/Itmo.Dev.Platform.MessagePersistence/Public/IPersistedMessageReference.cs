namespace Itmo.Dev.Platform.MessagePersistence;

public interface IPersistedMessageReference<out TMessage>
{
    DateTimeOffset CreatedAt { get; }

    TMessage Message { get; }

    void SetSuccessResult();

    void SetIgnoredResult();

    void SetFailedResult(Exception? exception = null);
}
