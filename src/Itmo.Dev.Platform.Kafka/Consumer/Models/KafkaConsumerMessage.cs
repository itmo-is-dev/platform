using Confluent.Kafka;
using Itmo.Dev.Platform.Kafka.Tools;
using Newtonsoft.Json;
using System.Text;

namespace Itmo.Dev.Platform.Kafka.Consumer.Models;

internal class KafkaConsumerMessage<TKey, TValue> : IKafkaConsumerMessage<TKey, TValue>
{
    private readonly IConsumer<TKey, TValue> _consumer;
    private readonly ConsumeResult<TKey, TValue> _result;

    public KafkaConsumerMessage(IConsumer<TKey, TValue> consumer, ConsumeResult<TKey, TValue> result)
    {
        _consumer = consumer;
        _result = result;

        Key = result.Message.Key;
        Value = result.Message.Value;
        Timestamp = new DateTimeOffset(result.Message.Timestamp.UtcDateTime);
        Topic = result.Topic;
        Partition = result.Partition;
        Offset = result.Offset;

        Headers = result.Message.Headers
            .Select(header => new KeyValuePair<string, string>(
                header.Key,
                Encoding.UTF8.GetString(header.GetValueBytes())))
            .ToList();
    }

    /// <summary>
    ///     Needed for inbox message deserialization.
    ///     Main constructor causes NRE from accessing result parameter.
    /// </summary>
    [JsonConstructor]
    private KafkaConsumerMessage(
        IConsumer<TKey, TValue> consumer,
        ConsumeResult<TKey, TValue> result,
        TKey key,
        TValue value,
        string topic,
        List<KeyValuePair<string, string>> headers)
    {
        _consumer = consumer;
        _result = result;
        Key = key;
        Value = value;
        Topic = topic;
        Headers = headers;
    }

    public TKey Key { get; }

    public TValue Value { get; }

    public DateTimeOffset Timestamp { get; }

    public string Topic { get; }

    public Partition Partition { get; }

    public Offset Offset { get; }

    public List<KeyValuePair<string, string>> Headers { get; }

    public void Commit()
    {
        _consumer.Commit(_result);
    }
}
