namespace Itmo.Dev.Platform.Kafka.Producer.Models;

internal class KafkaProducerConfiguration : IKafkaProducerConfiguration
{
    public string Host { get; private set; } = string.Empty;

    public string Topic { get; init; } = string.Empty;

    public IKafkaProducerConfiguration WithHost(string host)
    {
        Host = host;
        return this;
    }
}