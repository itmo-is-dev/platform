using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.MessagePersistence;
using Itmo.Dev.Platform.MessagePersistence.Internal.Tools;
using System.Diagnostics;
using MessagePersistenceConstants = Itmo.Dev.Platform.MessagePersistence.Internal.Tools.MessagePersistenceConstants;

namespace Itmo.Dev.Platform.Kafka.Producer.Outbox;

internal class AlwaysOutboxMessageProducer<TKey, TValue> : IKafkaMessageProducer<TKey, TValue>
{
    private readonly string _topicName;
    private readonly IMessagePersistenceService _service;

    public AlwaysOutboxMessageProducer(string topicName, IMessagePersistenceService service)
    {
        _topicName = topicName;
        _service = service;
    }

    public async Task ProduceAsync(
        IAsyncEnumerable<KafkaProducerMessage<TKey, TValue>> messages,
        CancellationToken cancellationToken)
    {
        using var activity = MessagePersistenceActivitySource.Value
            .StartActivity(
                name: MessagePersistenceConstants.Tracing.SpanName,
                ActivityKind.Internal,
                parentContext: default)
            .WithDisplayName($"[outbox] {_topicName}");

        var outboxMessages = await messages
            .Select(message => new OutboxPersistedMessage<TKey, TValue>
            {
                Key = message.Key,
                Value = message.Value,
                Headers = message.Headers,
            })
            .ToArrayAsync(cancellationToken);

        await _service.PersistAsync(outboxMessages, cancellationToken);
    }
}
