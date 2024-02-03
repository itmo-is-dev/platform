using Itmo.Dev.Platform.Kafka.Producer;

namespace Itmo.Dev.Platform.Kafka.Samples.Producer;

public class UsageScenario
{
    private readonly IKafkaMessageProducer<MessageKey, MessageValue> _producer;

    public UsageScenario(IKafkaMessageProducer<MessageKey, MessageValue> producer)
    {
        _producer = producer;
    }

    public async Task PublishMessagesAsync(IEnumerable<Message> messages, CancellationToken cancellationToken)
    {
        var producerMessages = messages
            .Select(message => new KafkaProducerMessage<MessageKey, MessageValue>(message.Key, message.Value))
            .ToAsyncEnumerable();

        await _producer.ProduceAsync(producerMessages, cancellationToken);
    }
}