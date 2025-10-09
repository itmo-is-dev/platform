using Itmo.Dev.Platform.MessagePersistence.Tests.Models;

namespace Itmo.Dev.Platform.MessagePersistence.Tests.Handlers;

public class FailingHandler<TKey, TValue> : IMessagePersistenceHandler<TKey, TValue>
{
    private readonly TestCollectionContext<TKey, TValue> _context;

    public FailingHandler(TestCollectionContext<TKey, TValue> context)
    {
        _context = context;
    }

    public ValueTask HandleAsync(IEnumerable<IMessage<TKey, TValue>> messages, CancellationToken cancellationToken)
    {
        foreach (IMessage<TKey, TValue> message in messages)
        {
            _context.Messages.Add(new TestMessage<TKey, TValue>(message.Key, message.Value));
            message.SetFailedResult();
        }

        return ValueTask.CompletedTask;
    }
}