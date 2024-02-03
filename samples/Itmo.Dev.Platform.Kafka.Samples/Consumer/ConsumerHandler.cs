using Itmo.Dev.Platform.Kafka.Consumer;

namespace Itmo.Dev.Platform.Kafka.Samples.Consumer;

public class ConsumerHandler : IKafkaConsumerHandler<MessageKey, MessageValue>
{
    public ValueTask HandleAsync(
        IEnumerable<IKafkaConsumerMessage<MessageKey, MessageValue>> messages,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Consumer received {messages.Count()} messages");
        return ValueTask.CompletedTask;
    }
}