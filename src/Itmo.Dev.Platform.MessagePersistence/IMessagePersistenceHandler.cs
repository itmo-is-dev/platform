namespace Itmo.Dev.Platform.MessagePersistence;

public interface IMessagePersistenceHandler<in TKey, in TValue>
{
    ValueTask HandleAsync(IEnumerable<IMessage<TKey, TValue>> messages, CancellationToken cancellationToken);
}