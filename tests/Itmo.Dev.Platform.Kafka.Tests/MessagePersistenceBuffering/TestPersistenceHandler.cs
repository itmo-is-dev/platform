using Itmo.Dev.Platform.Kafka.Tests.Tools;
using Itmo.Dev.Platform.MessagePersistence;

namespace Itmo.Dev.Platform.Kafka.Tests.MessagePersistenceBuffering;

public class TestPersistenceHandler<TKey, TValue> : IMessagePersistenceHandler<TKey, TValue>
{
    private readonly TestContext<TKey, TValue> _context;

    public TestPersistenceHandler(TestContext<TKey, TValue> context)
    {
        _context = context;
    }

    public ValueTask HandleAsync(
        IEnumerable<IMessage<TKey, TValue>> messages,
        CancellationToken cancellationToken)
    {
        var message = messages.Single();
        _context.Complete(new TestMessage<TKey, TValue>(message.Key, message.Value));

        return ValueTask.CompletedTask;
    }
}