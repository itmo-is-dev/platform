namespace Itmo.Dev.Platform.Kafka.Producer;

public interface IKafkaProducerConfiguration
{
    string Topic { get; }

    int MessageMaxBytes => 1_000_000;
}