namespace Itmo.Dev.Platform.MessagePersistence;

public static class MessagePersistenceExtensions
{
    public static void SetResult<TKey, TValue>(
        this IEnumerable<IMessage<TKey, TValue>> messages,
        MessageHandleResult result)
    {
        foreach (IMessage<TKey, TValue> message in messages)
        {
            message.SetResult(result);
        }
    }
}
