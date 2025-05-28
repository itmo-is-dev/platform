using Itmo.Dev.Platform.Kafka.Consumer;

namespace Itmo.Dev.Platform.Kafka.Extensions;

public static class ConsumerMessageExtensions
{
    public static IEnumerable<IKafkaConsumerMessage<TKey, TValue>> GetLatestBy<TKey, TValue, TSelector>(
        this IEnumerable<IKafkaConsumerMessage<TKey, TValue>> messages,
        Func<IKafkaConsumerMessage<TKey, TValue>, TSelector> selector)
        where TSelector : IEquatable<TSelector>
    {
        return messages.GroupBy(selector, GetLatestMessageInGroup);

        IKafkaConsumerMessage<TKey, TValue> GetLatestMessageInGroup(
            TSelector key,
            IEnumerable<IKafkaConsumerMessage<TKey, TValue>> selectorGroup)
        {
            return selectorGroup
                .GroupBy(message => message.Partition.Value, GetLatestMessageInPartition)
                .OrderByDescending(x => x.Timestamp)
                .First();
        }

        IKafkaConsumerMessage<TKey, TValue> GetLatestMessageInPartition(
            int partition,
            IEnumerable<IKafkaConsumerMessage<TKey, TValue>> partitionGroup)
        {
            return partitionGroup.MaxBy(x => x.Offset)!;
        }
    }

    public static IEnumerable<IKafkaConsumerMessage<TKey, TValue>> GetLatestByKey<TKey, TValue>(
        this IEnumerable<IKafkaConsumerMessage<TKey, TValue>> messages)
        where TKey : IEquatable<TKey>
    {
        return messages.GetLatestBy(x => x.Key);
    }

    public static IEnumerable<IKafkaConsumerMessage<TKey, TValue>> GetLatestByKey<TKey, TValue, TSelector>(
        this IEnumerable<IKafkaConsumerMessage<TKey, TValue>> messages,
        Func<TKey, TSelector> selector)
        where TSelector : IEquatable<TSelector>
    {
        return messages.GetLatestBy(message => selector.Invoke(message.Key));
    }
}
