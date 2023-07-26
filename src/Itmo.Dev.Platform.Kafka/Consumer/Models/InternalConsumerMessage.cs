using Confluent.Kafka;

namespace Itmo.Dev.Platform.Kafka.Consumer.Models;

internal class InternalConsumerMessage<TKey, TValue>
{
    private readonly ConsumeResult<TKey, TValue> _result;
    private readonly IConsumer<TKey, TValue> _consumer;

    public InternalConsumerMessage(ConsumeResult<TKey, TValue> result, IConsumer<TKey, TValue> consumer)
    {
        _result = result;
        _consumer = consumer;

        Message = new ConsumerKafkaMessage<TKey, TValue>(
            result.Message.Key,
            result.Message.Value,
            new DateTimeOffset(result.Message.Timestamp.UtcDateTime),
            result.Topic,
            result.Partition,
            result.Offset);
    }

    public ConsumerKafkaMessage<TKey, TValue> Message { get; }

    public void Commit()
    {
        _consumer.Commit(_result);
    }
}