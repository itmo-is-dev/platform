namespace Itmo.Dev.Platform.MessagePersistence.Tests.Models;

public class TestContext<TKey, TValue>
{
    private readonly TaskCompletionSource<TestMessage<TKey, TValue>> _tcs =
        new TaskCompletionSource<TestMessage<TKey, TValue>>();

    public Task<TestMessage<TKey, TValue>> Message => _tcs.Task;

    public void Complete(TestMessage<TKey, TValue> message)
        => _tcs.SetResult(message);
}