using Itmo.Dev.Platform.MessagePersistence;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Kafka.Producer.Outbox;

internal class FallbackOutboxMessageProducer<TKey, TValue> : IKafkaMessageProducer<TKey, TValue>
{
    private readonly string _topicName;
    private readonly IMessagePersistenceConsumer _consumer;
    private readonly IKafkaMessageProducer<TKey, TValue> _producer;

    public FallbackOutboxMessageProducer(string topicName, IServiceProvider serviceProvider)
    {
        _topicName = topicName;
        _consumer = serviceProvider.GetRequiredService<IMessagePersistenceConsumer>();
        _producer = serviceProvider.GetRequiredKeyedService<IKafkaMessageProducer<TKey, TValue>>(topicName);
    }

    public async Task ProduceAsync(
        IAsyncEnumerable<KafkaProducerMessage<TKey, TValue>> messages,
        CancellationToken cancellationToken)
    {
        var messagesArray = await messages.ToArrayAsync(cancellationToken);
        
        try
        {
            await _producer.ProduceAsync(messagesArray.ToAsyncEnumerable(), cancellationToken);
        }
        catch
        {
            var persistedMessages = messagesArray
                .Select(x => new PersistedMessage<TKey, TValue>(x.Key, x.Value))
                .ToArray();

            await _consumer.ConsumeAsync(
                KafkaOutboxMessageName.ForTopic(_topicName),
                persistedMessages,
                cancellationToken);
        }
    }
}