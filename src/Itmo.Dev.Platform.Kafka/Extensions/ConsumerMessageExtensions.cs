using Itmo.Dev.Platform.Kafka.Consumer;

namespace Itmo.Dev.Platform.Kafka.Extensions;

public static class ConsumerMessageExtensions
{
    public static IEnumerable<IKafkaConsumerMessage<TKey, TValue>> GetLatestBy<TKey, TValue, TSelector>(
        this IEnumerable<IKafkaConsumerMessage<TKey, TValue>> messages,
        Func<IKafkaConsumerMessage<TKey, TValue>, TSelector> selector)
        where TSelector : IComparable<TSelector>
    {
        return messages.GroupBy(
            selector,
            (_, group) => group
                .OrderByDescending(x => x.Timestamp)
                .ThenByDescending(x => x.Offset)
                .First());
    }

    public static IEnumerable<IKafkaConsumerMessage<TKey, TValue>> GetLatestByKey<TKey, TValue>(
        this IEnumerable<IKafkaConsumerMessage<TKey, TValue>> messages)
        where TKey : IComparable<TKey>
    {
        return messages.GetLatestBy(x => x.Key);
    }

    public static IEnumerable<IKafkaConsumerMessage<TKey, TValue>> GetLatestByKey<TKey, TValue, TSelector>(
        this IEnumerable<IKafkaConsumerMessage<TKey, TValue>> messages,
        Func<TKey, TSelector> selector)
        where TSelector : IComparable<TSelector>
    {
        return messages.GetLatestBy(message => selector.Invoke(message.Key));
    }
}
