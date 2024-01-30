namespace Itmo.Dev.Platform.MessagePersistence;

public record PersistedMessage<TKey, TValue>(TKey Key, TValue Value);