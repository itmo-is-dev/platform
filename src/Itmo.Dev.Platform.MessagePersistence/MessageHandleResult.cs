namespace Itmo.Dev.Platform.MessagePersistence;

public abstract record MessageHandleResult
{
    private MessageHandleResult() { }

    public sealed record Success : MessageHandleResult;

    public sealed record Ignored : MessageHandleResult;

    public sealed record Failure(Exception? Exception) : MessageHandleResult;
}
