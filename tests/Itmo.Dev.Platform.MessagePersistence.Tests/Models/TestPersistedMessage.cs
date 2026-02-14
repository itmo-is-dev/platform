namespace Itmo.Dev.Platform.MessagePersistence.Tests.Models;

public sealed class TestPersistedMessage<TKey, TValue> : IPersistedMessage<
    TestPersistedMessage<TKey, TValue>,
    TestPersistedMessage<TKey, TValue>.PayloadV1>
{
    public required TKey Key { get; init; }

    public required TValue Value { get; init; }

    public PayloadV1 Payload => new(Key, Value);

    public static IPersistedMessageFinalBuilder Configure(
        IPersistedMessageBuilder<TestPersistedMessage<TKey, TValue>> builder)
    {
        return builder
            .WithPayload<PayloadV1>()
            .CreatedWith(payload => new TestPersistedMessage<TKey, TValue>
            {
                Key = payload.Key,
                Value = payload.Value,
            });
    }

    public sealed record PayloadV1(TKey Key, TValue Value) : IPersistedMessagePayload<PayloadV1>
    {
        object IPersistedMessagePayload.Key => Key!;
    }
}

public static class TestPersistenceMessage
{
    public static TestPersistedMessage<TKey, TValue> Create<TKey, TValue>(TKey key, TValue value)
    {
        return new TestPersistedMessage<TKey, TValue>
        {
            Key = key,
            Value = value,
        };
    }
}
