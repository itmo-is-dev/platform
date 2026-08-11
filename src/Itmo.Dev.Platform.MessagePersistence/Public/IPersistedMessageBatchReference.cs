namespace Itmo.Dev.Platform.MessagePersistence;

public interface IPersistedMessageBatchReference<out TMessage> : IPersistedMessageReference<TMessage>;
