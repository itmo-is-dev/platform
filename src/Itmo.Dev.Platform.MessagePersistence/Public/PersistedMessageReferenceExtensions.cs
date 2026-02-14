namespace Itmo.Dev.Platform.MessagePersistence;

public static class PersistedMessageReferenceExtensions
{
    public static void SetSuccessResult<TMessage>(this IEnumerable<IPersistedMessageReference<TMessage>> messages)
    {
        foreach (IPersistedMessageReference<TMessage> message in messages)
        {
            message.SetSuccessResult();
        }
    }

    public static void SetIgnoredResult<TMessage>(this IEnumerable<IPersistedMessageReference<TMessage>> messages)
    {
        foreach (IPersistedMessageReference<TMessage> message in messages)
        {
            message.SetIgnoredResult();
        }
    }

    public static void SetFailedResult<TMessage>(
        this IEnumerable<IPersistedMessageReference<TMessage>> messages,
        Exception? exception = null)
    {
        foreach (IPersistedMessageReference<TMessage> message in messages)
        {
            message.SetFailedResult(exception);
        }
    }
}
