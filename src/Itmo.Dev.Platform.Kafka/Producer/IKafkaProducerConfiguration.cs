namespace Itmo.Dev.Platform.Kafka.Producer;

public interface IKafkaProducerConfiguration
{
    string Host { get; }
    
    string Topic { get; }

    int MessageMaxBytes => 1_000_000;
}