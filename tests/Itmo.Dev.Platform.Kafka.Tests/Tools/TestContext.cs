using Itmo.Dev.Platform.Kafka.Consumer;

namespace Itmo.Dev.Platform.Kafka.Tests.Tools;

public class TestContext<TKey, TValue>
{
    private readonly TaskCompletionSource<TestMessage<TKey, TValue>> _tcs = new();

    public Task<TestMessage<TKey, TValue>> Message => _tcs.Task;

    public ICollection<IKafkaConsumerMessage<TKey, TValue>> Messages { get; } =
        new List<IKafkaConsumerMessage<TKey, TValue>>();

    public void Complete(TestMessage<TKey, TValue> message)
        => _tcs.SetResult(message);
}
