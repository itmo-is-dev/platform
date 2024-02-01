namespace Itmo.Dev.Platform.Kafka.Consumer;

public interface IKafkaInboxHandler<in TKey, in TValue>
{
    ValueTask HandleAsync(IEnumerable<IKafkaInboxMessage<TKey, TValue>> messages, CancellationToken cancellationToken);
}