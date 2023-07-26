using Itmo.Dev.Platform.Kafka.Producer.Models;

namespace Itmo.Dev.Platform.Kafka.Producer;

public interface IKafkaMessageProducer<TKey, TValue>
{
    Task ProduceAsync(
        IAsyncEnumerable<ProducerKafkaMessage<TKey, TValue>> messages,
        CancellationToken cancellationToken);
}