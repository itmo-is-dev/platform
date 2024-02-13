namespace Itmo.Dev.Platform.MessagePersistence.Tests.Models;

public class TestCollectionContext<TKey, TValue>
{
    public List<TestMessage<TKey, TValue>> Messages { get; } = [];
}