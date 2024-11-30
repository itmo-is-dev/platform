using System.Collections.Concurrent;

namespace Itmo.Dev.Platform.Kafka.Producer;

internal static class KafkaOutboxMessageName
{
    private static readonly ConcurrentDictionary<string, string> Values = [];

    public static string ForTopic(string topicName)
        => Values.GetOrAdd(topicName, static topicName => $"_platform_kafka_outbox_{topicName}");
}