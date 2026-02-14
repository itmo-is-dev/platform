using Itmo.Dev.Platform.MessagePersistence.Tests.Models;

namespace Itmo.Dev.Platform.MessagePersistence.Tests.Handlers;

public class TestPersistedMessageHandler<TKey, TValue> : IPersistedMessageHandler<TestPersistedMessage<TKey, TValue>>
{
    private readonly TestContext<TKey, TValue> _context;

    public TestPersistedMessageHandler(TestContext<TKey, TValue> context)
    {
        _context = context;
    }

    public ValueTask HandleAsync(
        IEnumerable<IPersistedMessageReference<TestPersistedMessage<TKey, TValue>>> messages,
        CancellationToken cancellationToken)
    {
        var message = messages.Single();
        _context.Complete(new TestMessage<TKey, TValue>(message.Message.Key, message.Message.Value));

        return ValueTask.CompletedTask;
    }
}
