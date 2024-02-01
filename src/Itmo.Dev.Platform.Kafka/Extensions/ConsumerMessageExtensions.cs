using Itmo.Dev.Platform.Kafka.Consumer;

namespace Itmo.Dev.Platform.Kafka.Extensions;

public static class ConsumerMessageExtensions
{
    public static IEnumerable<IKafkaConsumerMessage<TKey, TValue>> GetLatestBy<TKey, TValue, TSelector>(
        this IEnumerable<IKafkaConsumerMessage<TKey, TValue>> messages,
        Func<IKafkaConsumerMessage<TKey, TValue>, TSelector> selector)
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
    {
        return messages.GetLatestBy(x => x.Key);
    }
}