using Itmo.Dev.Platform.Kafka.Consumer;
using Itmo.Dev.Platform.Kafka.Producer;
using Microsoft.Extensions.Configuration;

namespace Itmo.Dev.Platform.Kafka.MessagePersistence.Configuration;

public interface IKafkaBufferingProducerConfigurationSelector
{
    IKafkaBufferingConsumerConfigurationSelector WithProducerConfiguration(
        IConfiguration configuration,
        Action<KafkaProducerOptions>? action = null);
}

public interface IKafkaBufferingConsumerConfigurationSelector
{
    IKafkaBufferingBuilder WithConsumerConfiguration(
        IConfiguration configuration,
        Action<KafkaConsumerOptions>? action = null);
}

public interface IKafkaBufferingBuilder { }
