using Itmo.Dev.Platform.Common.Models;
using Itmo.Dev.Platform.MessagePersistence;
using Newtonsoft.Json;

namespace Itmo.Dev.Platform.Kafka.Producer.Outbox;

public sealed class OutboxPersistedMessage<TKey, TValue> : IPersistedMessage<
    OutboxPersistedMessage<TKey, TValue>,
    OutboxPersistedMessage<TKey, TValue>.PayloadV1>
{
    public required TKey Key { get; init; }

    public required TValue Value { get; init; }

    public required IDictionary<string, string>? Headers { get; init; }

    public PayloadV1 Payload => new()
    {
        Key = Key,
        Value = Value,
        Headers = Headers,
    };

    public static IPersistedMessageFinalBuilder Configure(
        IPersistedMessageBuilder<OutboxPersistedMessage<TKey, TValue>> builder)
    {
        return builder
            .WithPayload<PayloadV1>()
            .CreatedWith(payload => new OutboxPersistedMessage<TKey, TValue>
            {
                Key = payload.Key,
                Value = payload.Value,
                Headers = payload.Headers,
            });
    }

    public class PayloadV1 : IPersistedMessagePayload<PayloadV1>
    {
        [JsonIgnore]
        object IPersistedMessagePayload.Key => Unit.Value;

        public required TKey Key { get; init; }

        public required TValue Value { get; init; }

        public required IDictionary<string, string>? Headers { get; init; }
    }
}
