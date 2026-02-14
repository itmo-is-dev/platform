using Itmo.Dev.Platform.MessagePersistence;

namespace Itmo.Dev.Platform.Kafka.Tests.MessagePersistenceBuffering;

public sealed class TestPersistenceMessage<TKey, TValue> : IPersistedMessage<
    TestPersistenceMessage<TKey, TValue>,
    TestPersistenceMessage<TKey, TValue>.PayloadV1>
{
    public required TKey Key { get; init; }

    public required TValue Value { get; init; }

    public PayloadV1 Payload => new(Key, Value);

    public static IPersistedMessageFinalBuilder Configure(
        IPersistedMessageBuilder<TestPersistenceMessage<TKey, TValue>> builder)
    {
        return builder
            .WithPayload<PayloadV1>()
            .CreatedWith(payload => new TestPersistenceMessage<TKey, TValue>
            {
                Key = payload.Key,
                Value = payload.Value,
            });
    }

    public record PayloadV1(TKey Key, TValue Value) : IPersistedMessagePayload<PayloadV1>
    {
        object IPersistedMessagePayload.Key => Key!;
    }
}

public static class TestPersistenceMessage
{
    public static TestPersistenceMessage<TKey, TValue> Create<TKey, TValue>(TKey key, TValue value) => new()
    {
        Key = key,
        Value = value,
    };
}
