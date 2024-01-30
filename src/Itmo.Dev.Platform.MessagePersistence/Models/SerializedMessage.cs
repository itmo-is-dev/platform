namespace Itmo.Dev.Platform.MessagePersistence.Models;

public record SerializedMessage(
    long Id,
    string Name,
    DateTimeOffset CreatedAt,
    MessageState State,
    string Key,
    string Value);