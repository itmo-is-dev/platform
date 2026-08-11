namespace Itmo.Dev.Platform.MessagePersistence;

/// <summary>
///     A batch of messages that was group by specific key.
///     It's properties return data from the "latest" message in batch,
///     but the method calls are delegated to all messages in a batch.
/// </summary>
public interface IPersistedMessageBatchReference<out TMessage> : IPersistedMessageReference<TMessage>;
