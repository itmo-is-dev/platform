namespace Itmo.Dev.Platform.Kafka.Producer;

public interface IKafkaMessageProducer<TKey, TValue>
{
    Task ProduceAsync(
        IAsyncEnumerable<KafkaProducerMessage<TKey, TValue>> messages,
        CancellationToken cancellationToken);
}