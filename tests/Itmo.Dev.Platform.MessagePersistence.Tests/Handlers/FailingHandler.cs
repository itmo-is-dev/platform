using Itmo.Dev.Platform.MessagePersistence.Tests.Models;

namespace Itmo.Dev.Platform.MessagePersistence.Tests.Handlers;

public class FailingHandler<TKey, TValue> : IPersistedMessageHandler<TestPersistedMessage<TKey, TValue>>
{
    private readonly TestCollectionContext<TKey, TValue> _context;

    public FailingHandler(TestCollectionContext<TKey, TValue> context)
    {
        _context = context;
    }

    public ValueTask HandleAsync(
        IEnumerable<IPersistedMessageReference<TestPersistedMessage<TKey, TValue>>> messages,
        CancellationToken cancellationToken)
    {
        foreach (IPersistedMessageReference<TestPersistedMessage<TKey, TValue>> message in messages)
        {
            _context.Messages.Add(message.Message);
            message.SetFailedResult();
        }

        return ValueTask.CompletedTask;
    }
}
