namespace Itmo.Dev.Platform.Kafka.Consumer;

public interface IKafkaConsumerHandler<in TKey, in TValue>
{
    ValueTask HandleAsync(
        IEnumerable<IKafkaConsumerMessage<TKey, TValue>> messages,
        CancellationToken cancellationToken);
}