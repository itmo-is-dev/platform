namespace Itmo.Dev.Platform.MessagePersistence.Models;

internal class SerializedMessage
{
    public long Id { get; init; }
    public string Name { get; init; }
    public string Key { get; init; }
    public string Value { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public MessageState State { get; set; }
    public int RetryCount { get; set; }
    public string? BufferingStep { get; set; }
    public IDictionary<string, string> Headers { get; }

    public SerializedMessage(
        long id,
        string name,
        DateTimeOffset createdAt,
        MessageState state,
        string key,
        string value,
        int retryCount,
        string? bufferingStep,
        IDictionary<string, string> headers)
    {
        Id = id;
        Name = name;
        CreatedAt = createdAt;
        State = state;
        Key = key;
        Value = value;
        RetryCount = retryCount;
        BufferingStep = bufferingStep;
        Headers = headers;
    }
}
