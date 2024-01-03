using Confluent.Kafka;

namespace Itmo.Dev.Platform.Kafka.Consumer;

public interface IKafkaConsumerConfiguration
{
    bool IsDisabled { get; }

    TimeSpan DisabledConsumerTimeout { get; }

    string Topic { get; }

    string Group { get; }

    string InstanceId { get; }

    int ParallelismDegree { get; }

    int BufferSize { get; }

    TimeSpan BufferWaitLimit { get; }

    bool ReadLatest { get; }

    IKafkaConsumerConfiguration WithGroup(string group);

    IKafkaConsumerConfiguration WithInstanceId(string instanceId);
}