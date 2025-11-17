namespace Itmo.Dev.Platform.Kafka.Tools;

public static class PlatformKafkaConstants
{
    public static class Tracing
    {
        public const string TraceParentHeader = "traceparent";

        public const string TopicTag = "kafka_topic";
        public const string PartitionTag = "kafka_partition";
        public const string OffsetTag = "kafka_offset";
    }
}
