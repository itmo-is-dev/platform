using Itmo.Dev.Platform.Kafka.Consumer;
using Itmo.Dev.Platform.Kafka.Extensions;
using Itmo.Dev.Platform.Kafka.MessagePersistence.Models;
using Itmo.Dev.Platform.Kafka.Producer;
using Itmo.Dev.Platform.MessagePersistence;
using Itmo.Dev.Platform.MessagePersistence.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Kafka.MessagePersistence.Configuration;

internal class KafkaBufferingBuilder :
    IKafkaBufferingProducerConfigurationSelector,
    IKafkaBufferingConsumerConfigurationSelector,
    IKafkaBufferingBuilder
{
    private readonly IMessagePersistenceBufferingStepSelector _stepSelector;

    public KafkaBufferingBuilder(IMessagePersistenceBufferingStepSelector stepSelector)
    {
        _stepSelector = stepSelector;
    }

    public IKafkaBufferingConsumerConfigurationSelector WithProducerConfiguration(
        IConfiguration configuration,
        Action<KafkaProducerOptions>? action = null)
    {
        var topicSection = configuration.GetSection("Topic");
        var topicName = topicSection.Value;

        if (string.IsNullOrEmpty(topicName))
            throw new InvalidOperationException("Topic name is not specified");

        _stepSelector.Services.AddProducerInternal(producer => producer
            .WithKey<BufferedMessageKey>()
            .WithValue<BufferedMessageValue>()
            .WithConfiguration(configuration, action)
            .SerializeKeyWithNewtonsoft()
            .SerializeValueWithNewtonsoft());

        _stepSelector.Services.AddKeyedScoped(
            _stepSelector.BufferGroupName,
            (provider, _) => ActivatorUtilities.CreateInstance<KafkaBufferingStepPublisher>(provider, topicName));

        var step = new BufferStepOptions
        {
            Name = topicName,
            PublisherType = typeof(KafkaBufferingStepPublisher),
        };

        _stepSelector.WithStep(step);

        return this;
    }

    public IKafkaBufferingBuilder WithConsumerConfiguration(
        IConfiguration configuration,
        Action<KafkaConsumerOptions>? action = null)
    {
        _stepSelector.Services.AddConsumerInternal(consumer => consumer
            .WithKey<BufferedMessageKey>()
            .WithValue<BufferedMessageValue>()
            .WithConfiguration(configuration, action)
            .DeserializeKeyWithNewtonsoft()
            .DeserializeValueWithNewtonsoft()
            .HandleWith<KafkaBufferingStepConsumerHandler>());

        return this;
    }
}
