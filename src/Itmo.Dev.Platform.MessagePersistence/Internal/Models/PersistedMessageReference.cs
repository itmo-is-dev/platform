namespace Itmo.Dev.Platform.MessagePersistence.Internal.Models;

internal class PersistedMessageReference<TMessage> : IPersistedMessageReference<TMessage>
{
    public PersistedMessageReference(long id, DateTimeOffset createdAt, TMessage message, MessageHandleResult result)
    {
        Id = id;
        CreatedAt = createdAt;
        Message = message;
        Result = result;
    }

    public long Id { get; }

    public DateTimeOffset CreatedAt { get; }

    public TMessage Message { get; }

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
