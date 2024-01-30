namespace Itmo.Dev.Platform.MessagePersistence.Models;

internal record SerializedMessage(
    long Id,
    string Name,
    DateTimeOffset CreatedAt,
    MessageState State,
    string Key,
    string Value);