namespace Itmo.Dev.Platform.MessagePersistence;

public interface IMessagePersistenceService
{
    ValueTask PersistAsync<TMessage>(
        IReadOnlyCollection<TMessage> messages,
        CancellationToken cancellationToken)
        where TMessage : IPersistedMessage;

    internal IAsyncEnumerable<long> PersistInternalAsync<TMessage>(
        IReadOnlyCollection<TMessage> messages,
        CancellationToken cancellationToken)
        where TMessage : IPersistedMessage;
}
