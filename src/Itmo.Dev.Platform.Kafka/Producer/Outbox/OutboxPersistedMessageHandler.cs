using Itmo.Dev.Platform.MessagePersistence;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Kafka.Producer.Outbox;

internal class OutboxPersistedMessageHandler<TKey, TValue>
    : IPersistedMessageHandler<OutboxPersistedMessage<TKey, TValue>>
{
    private readonly IKafkaMessageProducer<TKey, TValue> _producer;

    public OutboxPersistedMessageHandler(string topicName, IServiceProvider provider)
    {
        _producer = provider.GetRequiredKeyedService<IKafkaMessageProducer<TKey, TValue>>(topicName);
    }

    public async ValueTask HandleAsync(
        IEnumerable<IPersistedMessageReference<OutboxPersistedMessage<TKey, TValue>>> messages,
        CancellationToken cancellationToken)
    {
        var producerMessages = messages
            .Select(handle => KafkaProducerMessage.Create(handle.Message.Key, handle.Message.Value))
            .ToAsyncEnumerable();

        await _producer.ProduceAsync(producerMessages, cancellationToken);
    }
}
