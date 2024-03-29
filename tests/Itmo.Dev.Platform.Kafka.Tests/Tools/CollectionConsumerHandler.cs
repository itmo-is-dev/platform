using Itmo.Dev.Platform.Kafka.Consumer;
using Serilog;

namespace Itmo.Dev.Platform.Kafka.Tests.Tools;

public class CollectionConsumerHandler<TKey, TValue> : IKafkaConsumerHandler<TKey, TValue>
{
    private readonly Guid _id = Guid.NewGuid();
    private readonly TestContext<TKey, TValue> _context;

    public CollectionConsumerHandler(TestContext<TKey, TValue> context)
    {
        _context = context;
    }

    public ValueTask HandleAsync(
        IEnumerable<IKafkaConsumerMessage<TKey, TValue>> messages,
        CancellationToken cancellationToken)
    {
        foreach (IKafkaConsumerMessage<TKey, TValue> message in messages)
        {
            _context.Messages.Add(message);
            Log.Information("Added message ({Count}), id = {Id}", _context.Messages.Count, _id);
        }

        return ValueTask.CompletedTask;
    }
}