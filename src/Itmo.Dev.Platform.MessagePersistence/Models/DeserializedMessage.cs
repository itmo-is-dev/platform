namespace Itmo.Dev.Platform.MessagePersistence.Models;

public record DeserializedMessage<TKey, TValue>(
    long Id,
    string Name,
    DateTimeOffset CreatedAt,
    MessageState State,
    TKey Key,
    TValue Value);