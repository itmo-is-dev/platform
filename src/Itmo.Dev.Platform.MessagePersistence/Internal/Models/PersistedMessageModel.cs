namespace Itmo.Dev.Platform.MessagePersistence.Internal.Models;

internal sealed class PersistedMessageModel
{
    public required long Id { get; init; }
    public required PayloadVersion Version { get; set; }
    public required string Name { get; init; }
    public required string Key { get; set; }
    public required string Value { get; set; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required MessageState State { get; set; }
    public required int RetryCount { get; set; }
    public required string? BufferingStep { get; set; }
    public required IDictionary<string, string> Headers { get; init; }
}
