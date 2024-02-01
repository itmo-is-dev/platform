using Itmo.Dev.Platform.MessagePersistence;

namespace Itmo.Dev.Platform.Kafka.Producer.Outbox;

internal class OutboxMessageProducer<TKey, TValue> : IKafkaMessageProducer<TKey, TValue>
{
    private readonly string _messageName;
    private readonly IMessagePersistenceConsumer _consumer;

    public OutboxMessageProducer(string messageName, IMessagePersistenceConsumer consumer)
    {
        _messageName = messageName;
        _consumer = consumer;
    }

    public async Task ProduceAsync(
        IAsyncEnumerable<KafkaProducerMessage<TKey, TValue>> messages,
        CancellationToken cancellationToken)
    {
        var persistedMessages = await messages
            .Select(x => new PersistedMessage<TKey, TValue>(x.Key, x.Value))
            .ToArrayAsync(cancellationToken);

        await _consumer.ConsumeAsync(_messageName, persistedMessages, cancellationToken);
    }
}