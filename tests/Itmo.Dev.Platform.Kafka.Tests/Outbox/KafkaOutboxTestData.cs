using Itmo.Dev.Platform.Kafka.Producer;

namespace Itmo.Dev.Platform.Kafka.Tests.Outbox;

public record KafkaOutboxTestData(int BufferSize, KafkaProducerMessage<int, string>[] Messages)
{
    public static KafkaOutboxTestData SingleMessage(int bufferSize)
        => new(bufferSize, [new KafkaProducerMessage<int, string>(1, "aboba")]);

    public static KafkaOutboxTestData Many(int bufferSize, int messageCount)
    {
        var messages = Enumerable
            .Range(0, messageCount)
            .Select(i => new KafkaProducerMessage<int, string>(i, i.ToString()))
            .ToArray();

        return new KafkaOutboxTestData(bufferSize, messages);
    }
}
