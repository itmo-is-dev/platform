using Itmo.Dev.Platform.Kafka.Consumer;

namespace Itmo.Dev.Platform.Kafka.Samples.ConsumerInbox;

public class InboxHandler : IKafkaInboxHandler<MessageKey, MessageValue>
{
    public ValueTask HandleAsync(
        IEnumerable<IKafkaInboxMessage<MessageKey, MessageValue>> messages,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Inbox consumer received {messages.Count()} messages");
        return ValueTask.CompletedTask;
    }
}