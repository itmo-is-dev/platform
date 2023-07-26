using Itmo.Dev.Platform.Kafka.Consumer.Models;

namespace Itmo.Dev.Platform.Kafka.Consumer;

public interface IKafkaMessageHandler<TKey, TValue>
{
    ValueTask HandleAsync(IEnumerable<ConsumerKafkaMessage<TKey, TValue>> messages, CancellationToken cancellationToken);
}