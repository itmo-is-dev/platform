using Itmo.Dev.Platform.Kafka.Consumer;
using Itmo.Dev.Platform.Kafka.Consumer.Models;
using Serilog;

namespace Itmo.Dev.Platform.Kafka.Tests.Tools;

public class CollectionConsumerHandler<TKey, TValue> : IKafkaMessageHandler<TKey, TValue>
{
    private readonly ICollection<ConsumerKafkaMessage<TKey, TValue>> _collection;
    private readonly Guid _id = Guid.NewGuid();

    public CollectionConsumerHandler(ICollection<ConsumerKafkaMessage<TKey, TValue>> collection)
    {
        _collection = collection;
    }

    public ValueTask HandleAsync(
        IEnumerable<ConsumerKafkaMessage<TKey, TValue>> messages,
        CancellationToken cancellationToken)
    {
        foreach (ConsumerKafkaMessage<TKey, TValue> message in messages)
        {
            _collection.Add(message);
            Log.Information("Added message ({Count}), id = {Id}", _collection.Count, _id);
        }

        return ValueTask.CompletedTask;
    }
}