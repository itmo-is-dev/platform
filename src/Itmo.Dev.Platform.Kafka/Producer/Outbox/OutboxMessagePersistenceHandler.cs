using Itmo.Dev.Platform.MessagePersistence;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Kafka.Producer.Outbox;

internal class OutboxMessagePersistenceHandler<TKey, TValue> : IMessagePersistenceHandler<TKey, TValue>
{
    private readonly IKafkaMessageProducer<TKey, TValue> _producer;

    public OutboxMessagePersistenceHandler(string topicName, IServiceProvider provider)
    {
        _producer = provider.GetRequiredKeyedService<IKafkaMessageProducer<TKey, TValue>>(topicName);
    }

    public async ValueTask HandleAsync(IEnumerable<IMessage<TKey, TValue>> messages, CancellationToken cancellationToken)
    {
        var producerMessages = messages
            .Select(x => new KafkaProducerMessage<TKey, TValue>(x.Key, x.Value))
            .ToAsyncEnumerable();

        await _producer.ProduceAsync(producerMessages, cancellationToken);
    }
}