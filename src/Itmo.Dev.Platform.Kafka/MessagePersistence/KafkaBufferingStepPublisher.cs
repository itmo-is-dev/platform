using Itmo.Dev.Platform.Kafka.MessagePersistence.Models;
using Itmo.Dev.Platform.Kafka.Producer;
using Itmo.Dev.Platform.MessagePersistence.Buffering;
using Itmo.Dev.Platform.MessagePersistence.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Kafka.MessagePersistence;

internal class KafkaBufferingStepPublisher : IBufferingStepPublisher
{
    private readonly IKafkaMessageProducer<BufferedMessageKey, BufferedMessageValue> _producer;

    public KafkaBufferingStepPublisher(string topicName, IServiceProvider provider)
    {
        _producer = provider.GetRequiredKeyedService<IKafkaMessageProducer<BufferedMessageKey, BufferedMessageValue>>(
            topicName);
    }

    public async Task PublishAsync(IEnumerable<SerializedMessage> messages, CancellationToken cancellationToken)
    {
        var producerMessages = messages
            .Select(message => KafkaProducerMessage.Create(
                BufferedMessageKey.FromSerializedMessage(message),
                BufferedMessageValue.FromSerializedMessage(message)))
            .ToAsyncEnumerable();

        await _producer.ProduceAsync(producerMessages, cancellationToken);
    }
}
