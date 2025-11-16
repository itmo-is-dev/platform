using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.MessagePersistence;
using Itmo.Dev.Platform.MessagePersistence.Tools;
using System.Diagnostics;

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
        using var activity = PlatformMessagePersistenceActivitySource.Value
            .StartActivity(
                name: PlatformMessagePersistenceConstants.SpanName,
                ActivityKind.Internal,
                parentContext: default)
            .WithDisplayName($"[outbox] {_topicName}");
        
        var persistedMessages = await messages
            .Select(x => new PersistedMessage<TKey, TValue>(x.Key, x.Value))
            .ToArrayAsync(cancellationToken);

        await _consumer.ConsumeAsync(KafkaOutboxMessageName.ForTopic(_topicName), persistedMessages, cancellationToken);
    }
}