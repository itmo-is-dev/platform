using Itmo.Dev.Platform.Kafka.Consumer;
using Serilog;

namespace Itmo.Dev.Platform.Kafka.Tests.Tools;

public class CollectionInboxHandler<TKey, TValue> : IKafkaInboxHandler<TKey, TValue>
{
    private readonly Guid _id = Guid.NewGuid();
    private readonly TestContext<TKey, TValue> _context;

    public CollectionInboxHandler(TestContext<TKey, TValue> context)
    {
        _context = context;
    }

    public ValueTask HandleAsync(
        IEnumerable<IKafkaInboxMessage<TKey, TValue>> messages,
        CancellationToken cancellationToken)
    {
        foreach (var message in messages)
        {
            _context.Messages.Add(message);
            Log.Information("Added message ({Count}), id = {Id}", _context.Messages.Count, _id);
        }

        return ValueTask.CompletedTask;
    }
}