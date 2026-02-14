using Itmo.Dev.Platform.MessagePersistence.Tests.Models;

namespace Itmo.Dev.Platform.MessagePersistence.Tests.Handlers;

public sealed class TestVersionedPersistedMessageHandler<TKey, TValue>(
    TestContext<TKey, TValue> context
)
    : IPersistedMessageHandler<TestVersionedPersistedMessage<TKey, TValue>>
{
    public ValueTask HandleAsync(
        IEnumerable<IPersistedMessageReference<TestVersionedPersistedMessage<TKey, TValue>>> messages,
        CancellationToken cancellationToken)
    {
        var message = messages.Single();
        context.Complete(new TestMessage<TKey, TValue>(message.Message.Key, message.Message.Value));

        return ValueTask.CompletedTask;
    }
}
