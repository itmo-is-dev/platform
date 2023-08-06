using Itmo.Dev.Platform.Kafka.Consumer.Models;

namespace Itmo.Dev.Platform.Kafka.Extensions;

public static class ConsumerMessageExtensions
{
    public static IEnumerable<ConsumerKafkaMessage<TKey, TValue>> GetLatestBy<TKey, TValue, TSelector>(
        this IEnumerable<ConsumerKafkaMessage<TKey, TValue>> messages,
        Func<ConsumerKafkaMessage<TKey, TValue>, TSelector> selector)
    {
        return messages.GroupBy(
            selector,
            (_, group) => group
                .OrderByDescending(x => x.Timestamp)
                .ThenByDescending(x => x.Offset)
                .First());
    }

    public static IEnumerable<ConsumerKafkaMessage<TKey, TValue>> GetLatestByKey<TKey, TValue>(
        this IEnumerable<ConsumerKafkaMessage<TKey, TValue>> messages)
    {
        return messages.GetLatestBy(x => x.Key);
    }
}