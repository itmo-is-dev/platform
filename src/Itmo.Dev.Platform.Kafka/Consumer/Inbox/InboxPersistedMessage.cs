using Confluent.Kafka;
using Itmo.Dev.Platform.Common.Models;
using Itmo.Dev.Platform.MessagePersistence;
using Newtonsoft.Json;

namespace Itmo.Dev.Platform.Kafka.Consumer.Inbox;

public sealed class InboxPersistedMessage<TKey, TValue> : IPersistedMessage<
    InboxPersistedMessage<TKey, TValue>,
    InboxPersistedMessage<TKey, TValue>.PayloadV1>
{
    public required TKey Key { get; init; }
    public required TValue Value { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public required string Topic { get; init; }
    public required Partition Partition { get; init; }
    public required Offset Offset { get; init; }
    public required IList<KeyValuePair<string, string>> Headers { get; init; }

    public PayloadV1 Payload => new()
    {
        Key = Key,
        Value = Value,
        Timestamp = Timestamp,
        Topic = Topic,
        Partition = Partition,
        Offset = Offset,
        Headers = Headers,
    };

    public static IPersistedMessageFinalBuilder Configure(
        IPersistedMessageBuilder<InboxPersistedMessage<TKey, TValue>> builder)
    {
        return builder
            .WithPayload<PayloadV1>()
            .CreatedWith(payload => new InboxPersistedMessage<TKey, TValue>
            {
                Key = payload.Key,
                Value = payload.Value,
                Timestamp = payload.Timestamp,
                Topic = payload.Topic,
                Partition = payload.Partition,
                Offset = payload.Offset,
                Headers = payload.Headers,
            });
    }

    public class PayloadV1 : IPersistedMessagePayload<PayloadV1>
    {
        [JsonIgnore]
        object IPersistedMessagePayload.Key => Unit.Value;

        public required TKey Key { get; init; }
        public required TValue Value { get; init; }
        public required DateTimeOffset Timestamp { get; init; }
        public required string Topic { get; init; }
        public required Partition Partition { get; init; }
        public required Offset Offset { get; init; }
        public required IList<KeyValuePair<string, string>> Headers { get; init; }
    }
}
