using Confluent.Kafka;

namespace Itmo.Dev.Platform.Kafka.Consumer;

public interface IKafkaConsumerConfiguration
{
    bool IsDisabled { get; }

    TimeSpan DisabledConsumerTimeout { get; }

    string Host { get; }

    string Topic { get; }

    string Group { get; }

    int ParallelismDegree { get; }

    int BufferSize { get; }

    TimeSpan BufferWaitLimit { get; }

    bool ReadLatest { get; }

    SecurityProtocol SecurityProtocol { get; }

    IKafkaConsumerConfiguration WithHost(string host);

    IKafkaConsumerConfiguration WithGroup(string group);
}