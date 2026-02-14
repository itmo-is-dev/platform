namespace Itmo.Dev.Platform.MessagePersistence.Internal.Models;

internal class PersistedMessageCreateModel
{
    public required PayloadVersion Version { get; init; }
    public required string Name { get; init; }
    public required string Key { get; init; }
    public required string Value { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required MessageState State { get; set; }
    public required IDictionary<string, string> Headers { get; init; }
}
