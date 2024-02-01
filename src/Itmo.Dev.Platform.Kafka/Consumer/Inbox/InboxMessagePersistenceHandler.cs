using Itmo.Dev.Platform.Common.Models;
using Itmo.Dev.Platform.Kafka.Consumer.Models;
using Itmo.Dev.Platform.MessagePersistence;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Kafka.Consumer.Inbox;

internal class InboxMessagePersistenceHandler<TKey, TValue> :
    IMessagePersistenceHandler<Unit, KafkaConsumerMessage<TKey, TValue>>
{
    private readonly IKafkaInboxHandler<TKey, TValue> _handler;

    public InboxMessagePersistenceHandler(string topicName, IServiceProvider provider)
    {
        _handler = provider.GetRequiredKeyedService<IKafkaInboxHandler<TKey, TValue>>(topicName);
    }

    public async ValueTask HandleAsync(
        IEnumerable<IMessage<Unit, KafkaConsumerMessage<TKey, TValue>>> messages,
        CancellationToken cancellationToken)
    {
        var inboxMessages = messages
            .Select(x => new KafkaInboxMessage<TKey, TValue>(x));

        await _handler.HandleAsync(inboxMessages, cancellationToken);
    }
}