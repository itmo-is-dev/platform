using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.Common.Models;
using Itmo.Dev.Platform.Kafka.Consumer.Models;
using Itmo.Dev.Platform.MessagePersistence;
using Itmo.Dev.Platform.MessagePersistence.Tools;
using System.Diagnostics;

namespace Itmo.Dev.Platform.Kafka.Consumer.Inbox;

internal class InboxConsumerHandler<TKey, TValue> : IKafkaConsumerHandler<TKey, TValue>
{
    private readonly string _messageName;
    private readonly IMessagePersistenceConsumer _persistenceConsumer;

    public InboxConsumerHandler(string messageName, IMessagePersistenceConsumer persistenceConsumer)
    {
        _messageName = messageName;
        _persistenceConsumer = persistenceConsumer;
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

        var persistedMessages = consumerMessages
            .Select(message => new PersistedMessage<Unit, KafkaConsumerMessage<TKey, TValue>>(Unit.Value, message))
            .ToArray();

        await _persistenceConsumer.ConsumeAsync(_messageName, persistedMessages, cancellationToken);
    }
}