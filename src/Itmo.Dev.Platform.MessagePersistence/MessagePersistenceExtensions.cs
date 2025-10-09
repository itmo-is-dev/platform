namespace Itmo.Dev.Platform.MessagePersistence;

public static class MessagePersistenceExtensions
{
    public static void SetSuccessResult<TKey, TValue>(this IEnumerable<IMessage<TKey, TValue>> messages)
    {
        foreach (IMessage<TKey, TValue> message in messages)
        {
            message.SetSuccessResult();
        }
    }

    public static void SetIgnoredResult<TKey, TValue>(this IEnumerable<IMessage<TKey, TValue>> messages)
    {
        foreach (IMessage<TKey, TValue> message in messages)
        {
            message.SetIgnoredResult();
        }
    }

    public static void SetFailedResult<TKey, TValue>(
        this IEnumerable<IMessage<TKey, TValue>> messages,
        Exception? exception = null)
    {
        foreach (IMessage<TKey, TValue> message in messages)
        {
            message.SetFailedResult(exception);
        }
    }
}
