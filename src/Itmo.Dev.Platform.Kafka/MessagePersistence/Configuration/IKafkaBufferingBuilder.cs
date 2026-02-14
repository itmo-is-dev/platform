using Itmo.Dev.Platform.Kafka.Consumer;
using Itmo.Dev.Platform.Kafka.Producer;
using Microsoft.Extensions.Configuration;

namespace Itmo.Dev.Platform.Kafka.MessagePersistence.Configuration;

public interface IKafkaBufferingBuilder
{
    interface IConfigurationStep
    {
        IConsumerStep WithProducerConfiguration(
            IConfiguration configuration,
            Action<KafkaProducerOptions>? action = null);
    }

    interface IConsumerStep
    {
        IFailureStep WithConsumerConfiguration(
            IConfiguration configuration,
            Action<KafkaConsumerOptions>? action = null);
    }

    interface IFailureStep : IKafkaBufferingBuilder
    {
        IKafkaBufferingBuilder WithFailureBlockingBehaviour();
    }
}
