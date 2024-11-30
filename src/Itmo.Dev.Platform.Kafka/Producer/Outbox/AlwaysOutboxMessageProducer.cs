using Itmo.Dev.Platform.MessagePersistence;

namespace Itmo.Dev.Platform.Kafka.Producer.Outbox;

internal class AlwaysOutboxMessageProducer<TKey, TValue> : IKafkaMessageProducer<TKey, TValue>
{
    private readonly string _topicName;
    private readonly IMessagePersistenceConsumer _consumer;

    public AlwaysOutboxMessageProducer(string topicName, IMessagePersistenceConsumer consumer)
    {
        _topicName = topicName;
        _consumer = consumer;
    }

    public async Task ProduceAsync(
        IAsyncEnumerable<KafkaProducerMessage<TKey, TValue>> messages,
        CancellationToken cancellationToken)
    {
        var persistedMessages = await messages
            .Select(x => new PersistedMessage<TKey, TValue>(x.Key, x.Value))
            .ToArrayAsync(cancellationToken);

        await _consumer.ConsumeAsync(KafkaOutboxMessageName.ForTopic(_topicName), persistedMessages, cancellationToken);
    }
}