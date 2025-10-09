using Itmo.Dev.Platform.Kafka.Producer;
using System.Collections;
using System.Diagnostics;

namespace Itmo.Dev.Platform.Kafka.Tests.Outbox;

public record KafkaOutboxTestData(
    int TestKey,
    int BufferSize,
    KafkaOutboxTestData.TestDataMessageCollection Messages)
{
    private static int _testKey;

    public static KafkaOutboxTestData SingleMessage(int bufferSize)
        => Many(bufferSize, 1);

    public static KafkaOutboxTestData Many(int bufferSize, int messageCount)
    {
        var testKey = Interlocked.Increment(ref _testKey);

        var messages = Enumerable
            .Range(0, messageCount)
            .Select(i => new KafkaProducerMessage<int, string>(testKey, i.ToString()))
            .ToArray();

        return new KafkaOutboxTestData(testKey, bufferSize, messages);
    }

    [DebuggerDisplay("{ToString(),nq}")]
    public class TestDataMessageCollection(IReadOnlyCollection<KafkaProducerMessage<int, string>> messages)
        : IReadOnlyCollection<KafkaProducerMessage<int, string>>
    {
        public IEnumerator<KafkaProducerMessage<int, string>> GetEnumerator()
        {
            return messages.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return messages.GetEnumerator();
        }

        public int Count => messages.Count;

        public static implicit operator TestDataMessageCollection(KafkaProducerMessage<int, string>[] messages)
            => new(messages);

        public override string ToString()
            => $"[{Count}]";
    }
}
