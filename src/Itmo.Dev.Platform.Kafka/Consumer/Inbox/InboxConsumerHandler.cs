using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.Kafka.Consumer.Models;
using Itmo.Dev.Platform.MessagePersistence;
using Itmo.Dev.Platform.MessagePersistence.Internal.Tools;
using System.Diagnostics;

namespace Itmo.Dev.Platform.Kafka.Consumer.Inbox;

internal class InboxConsumerHandler<TKey, TValue> : IKafkaConsumerHandler<TKey, TValue>
{
    private readonly string _messageName;
    private readonly IMessagePersistenceService _persistenceService;

    public InboxConsumerHandler(string messageName, IMessagePersistenceService persistenceService)
    {
        _messageName = messageName;
        _persistenceService = persistenceService;
    }

    public async ValueTask HandleAsync(
        IEnumerable<IKafkaConsumerMessage<TKey, TValue>> messages,
        CancellationToken cancellationToken)
    {
        if (messages is not IEnumerable<KafkaConsumerMessage<TKey, TValue>> consumerMessages)
        {
            var expectedType = typeof(KafkaConsumerMessage<TKey, TValue>);

            throw new InvalidOperationException(
                $"Failed to write inbox message, invalid message type, expected = {expectedType}. You probably attempting some unaccounted platform tampering");
        }

        using var activity = MessagePersistenceActivitySource.Value
            .StartActivity(
                name: MessagePersistenceConstants.Tracing.SpanName,
                ActivityKind.Internal,
                parentContext: default)
            .WithDisplayName($"[inbox] {_messageName}");

        var inboxMessages = consumerMessages
            .Select(message => new InboxPersistedMessage<TKey, TValue>
            {
                Key = message.Key,
                Value = message.Value,
                Timestamp = message.Timestamp,
                Topic = message.Topic,
                Partition = message.Partition,
                Offset = message.Offset,
                Headers = message.Headers,
            })
            .ToArray();

        await _persistenceService.PersistAsync(inboxMessages, cancellationToken);
    }
}
