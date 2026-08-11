using Itmo.Dev.Platform.MessagePersistence.Internal.Models;

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

    /// <summary>
    ///     Groups messages by specified key and determines the "latest" message
    ///     calling MaxBy by specified ordering selector.
    /// </summary>
    /// <returns>
    ///     A message reference batch, which properties will return data from "latest" message,
    ///     but actions over it (such as result specification) will be performed over all messages in a batch.
    /// </returns>
    public static IEnumerable<IPersistedMessageBatchReference<TMessage>> BatchBy<TMessage, TKey, TOrder>(
        this IEnumerable<IPersistedMessageReference<TMessage>> messages,
        Func<IPersistedMessageReference<TMessage>, TKey> keySelector,
        Func<IPersistedMessageReference<TMessage>, TOrder> orderingSelector)
        where TKey : IEquatable<TKey>
        where TOrder : IComparable<TOrder>
    {
        return messages.GroupBy(
            keySelector,
            (_, group) =>
            {
                var messageCollection = group.ToArray();

                return new PersistedMessageBatchReference<TMessage>(
                    messageCollection.MaxBy(orderingSelector)!,
                    messageCollection);
            });
    }

    /// <summary>
    ///     Groups messages by specified key and determines the "latest" message
    ///     calling MaxBy over CreatedAt property.
    /// </summary>
    /// <returns>
    ///     A message reference batch, which properties will return data from "latest" message,
    ///     but actions over it (such as result specification) will be performed over all messages in a batch.
    /// </returns>
    public static IEnumerable<IPersistedMessageBatchReference<TMessage>> BatchBy<TMessage, TKey>(
        this IEnumerable<IPersistedMessageReference<TMessage>> messages,
        Func<IPersistedMessageReference<TMessage>, TKey> keySelector)
        where TKey : IEquatable<TKey>
    {
        return messages.BatchBy(keySelector, message => message.CreatedAt);
    }
}
