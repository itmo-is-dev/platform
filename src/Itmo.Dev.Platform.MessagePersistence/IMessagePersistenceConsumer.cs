namespace Itmo.Dev.Platform.MessagePersistence;

public interface IMessagePersistenceConsumer
{
    ValueTask ConsumeAsync<TKey, TValue>(
        string messageName,
        IReadOnlyCollection<PersistedMessage<TKey, TValue>> messages,
        CancellationToken cancellationToken);
}