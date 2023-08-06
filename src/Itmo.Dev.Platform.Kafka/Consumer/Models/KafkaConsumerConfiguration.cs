namespace Itmo.Dev.Platform.Kafka.Consumer.Models;

internal class KafkaConsumerConfiguration : IKafkaConsumerConfiguration
{
    public bool IsDisabled { get; init; }

    public TimeSpan DisabledConsumerTimeout { get; init; }

    public string Host { get; init; } = string.Empty;

    public string Topic { get; init; } = string.Empty;

    public string Group { get; init; } = string.Empty;

    public int ParallelismDegree { get; init; }

    public int BufferSize { get; init; }

    public TimeSpan BufferWaitLimit { get; init; }

    public bool ReadLatest { get; init; }
}