namespace Itmo.Dev.Platform.MessagePersistence.Tests.Models;

public class TestCollectionContext<TKey, TValue>
{
    public List<TestPersistedMessage<TKey, TValue>> Messages { get; } = [];
}
