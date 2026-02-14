using Itmo.Dev.Platform.Kafka.Consumer.Models;
using Itmo.Dev.Platform.MessagePersistence;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Kafka.Consumer.Inbox;

internal class InboxPersistedMessageHandler<TKey, TValue>
    : IPersistedMessageHandler<InboxPersistedMessage<TKey, TValue>>
{
    private readonly IKafkaInboxHandler<TKey, TValue> _handler;

    public InboxPersistedMessageHandler(string topicName, IServiceProvider provider)
    {
        _handler = provider.GetRequiredKeyedService<IKafkaInboxHandler<TKey, TValue>>(topicName);
    }

    public async ValueTask HandleAsync(
        IEnumerable<IPersistedMessageReference<InboxPersistedMessage<TKey, TValue>>> messages,
        CancellationToken cancellationToken)
    {
        var inboxMessages = messages
            .Select(x => new KafkaInboxMessage<TKey, TValue>(x));

        await _handler.HandleAsync(inboxMessages, cancellationToken);
    }
}
