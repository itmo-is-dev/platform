using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.MessagePersistence;
using Itmo.Dev.Platform.MessagePersistence.Internal.Tools;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using MessagePersistenceConstants = Itmo.Dev.Platform.MessagePersistence.Internal.Tools.MessagePersistenceConstants;

namespace Itmo.Dev.Platform.Kafka.Producer.Outbox;

internal class FallbackOutboxMessageProducer<TKey, TValue> : IKafkaMessageProducer<TKey, TValue>
{
    private readonly string _topicName;
    private readonly IMessagePersistenceService _service;
    private readonly IKafkaMessageProducer<TKey, TValue> _producer;

    public FallbackOutboxMessageProducer(string topicName, IServiceProvider serviceProvider)
    {
        _topicName = topicName;
        _service = serviceProvider.GetRequiredService<IMessagePersistenceService>();
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
            using var activity = MessagePersistenceActivitySource.Value
                .StartActivity(
                    name: MessagePersistenceConstants.Tracing.SpanName,
                    ActivityKind.Internal,
                    parentContext: default)
                .WithDisplayName($"[outbox] {_topicName}");

            var outboxMessages = messagesArray
                .Select(message => new OutboxPersistedMessage<TKey, TValue>
                {
                    Key = message.Key,
                    Value = message.Value,
                    Headers = message.Headers,
                })
                .ToArray();

            await _service.PersistAsync(outboxMessages, cancellationToken);
        }
    }
}
