namespace Itmo.Dev.Platform.MessagePersistence.Internal.Models;

internal sealed class PersistedMessageBatchReference<TMessage>(
    IPersistedMessageReference<TMessage> primaryMessage,
    IReadOnlyCollection<IPersistedMessageReference<TMessage>> messages)
    : IPersistedMessageBatchReference<TMessage>
{
    public DateTimeOffset CreatedAt => primaryMessage.CreatedAt;
    public TMessage Message => primaryMessage.Message;

    public void SetSuccessResult()
    {
        foreach (IPersistedMessageReference<TMessage> message in messages)
        {
            message.SetSuccessResult();
        }
    }

    public void SetIgnoredResult()
    {
        foreach (IPersistedMessageReference<TMessage> message in messages)
        {
            message.SetIgnoredResult();
        }
    }

    public void SetFailedResult(Exception? exception = null)
    {
        foreach (IPersistedMessageReference<TMessage> message in messages)
        {
            message.SetFailedResult(exception);
        }
    }
}
