using Confluent.Kafka;

namespace Itmo.Dev.Platform.Kafka.Consumer.Models;

internal class KafkaConsumerConfiguration : IKafkaConsumerConfiguration
{
    public bool IsDisabled { get; init; }

    public TimeSpan DisabledConsumerTimeout { get; init; }

    public string Host { get; private set; } = string.Empty;

    public string Topic { get; init; } = string.Empty;

    public string Group { get; private set; } = string.Empty;

    public int ParallelismDegree { get; init; }

    public int BufferSize { get; init; }

    public TimeSpan BufferWaitLimit { get; init; }

    public bool ReadLatest { get; init; }

    public SecurityProtocol SecurityProtocol { get; init; }

    public IKafkaConsumerConfiguration WithHost(string host)
    {
        Host = host;
        return this;
    }

    public IKafkaConsumerConfiguration WithGroup(string group)
    {
        Group = group;
        return this;
    }
}