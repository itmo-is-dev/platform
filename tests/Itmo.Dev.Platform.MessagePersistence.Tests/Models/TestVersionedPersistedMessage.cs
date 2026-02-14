namespace Itmo.Dev.Platform.MessagePersistence.Tests.Models;

public sealed class TestVersionedPersistedMessage<TKey, TValue> : IPersistedMessage<
    TestVersionedPersistedMessage<TKey, TValue>,
    TestVersionedPersistedMessage<TKey, TValue>.PayloadV2>
{
    public required TKey Key { get; init; }

    public required TValue Value { get; init; }

    public PayloadV2 Payload => new(Key, Value);

    public static IPersistedMessageFinalBuilder Configure(
        IPersistedMessageBuilder<TestVersionedPersistedMessage<TKey, TValue>> builder)
    {
        return builder
            .WithPayload<PayloadV1>()
            .WithPayload<PayloadV2>(version: 1)
            .CreatedWith(payload => new()
            {
                Key = payload.NewKey,
                Value = payload.NewValue,
            });
    }

    public record PayloadV1(TKey Key, TValue Value) : IPersistedMessagePayload<PayloadV1>
    {
        object IPersistedMessagePayload.Key => Key!;
    }

    public record PayloadV2(TKey NewKey, TValue NewValue) : IPersistedMessagePayload<PayloadV2, PayloadV1>
    {
        object IPersistedMessagePayload.Key => NewKey!;

        public static ValueTask<PayloadV2> MigrateAsync(
            PayloadV1 previous,
            IServiceProvider provider,
            CancellationToken cancellationToken)
        {
            var next = new PayloadV2(previous.Key, previous.Value);
            return ValueTask.FromResult(next);
        }
    }
}
