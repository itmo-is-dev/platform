using Itmo.Dev.Platform.Kafka.Consumer;

namespace Itmo.Dev.Platform.Kafka.Tests.Tools;

public class TestContext<TKey, TValue>
{
    public ICollection<IKafkaConsumerMessage<TKey, TValue>> Messages { get; } =
        new List<IKafkaConsumerMessage<TKey, TValue>>();
}