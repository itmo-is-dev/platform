namespace Itmo.Dev.Platform.MessagePersistence;

public interface IPersistedMessageHandler<TMessage>
    where TMessage : IPersistedMessage
{
    ValueTask HandleAsync(
        IEnumerable<IPersistedMessageReference<TMessage>> messages,
        CancellationToken cancellationToken);
}
