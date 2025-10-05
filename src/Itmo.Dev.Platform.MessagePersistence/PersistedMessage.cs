namespace Itmo.Dev.Platform.MessagePersistence;

public record PersistedMessage<TKey, TValue>(TKey Key, TValue Value);

public static class PersistedMessage
{
    public static PersistedMessage<TKey, TValue> Create<TKey, TValue>(TKey key, TValue value) => new(key, value);
}
