using Itmo.Dev.Platform.Kafka.Consumer;
using Itmo.Dev.Platform.Kafka.Extensions;
using Itmo.Dev.Platform.Kafka.MessagePersistence.Models;
using Itmo.Dev.Platform.Kafka.Producer;
using Itmo.Dev.Platform.MessagePersistence;
using Itmo.Dev.Platform.MessagePersistence.Internal.Execution.FailureProcessors;
using Itmo.Dev.Platform.MessagePersistence.Internal.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static Itmo.Dev.Platform.Kafka.MessagePersistence.Configuration.IKafkaBufferingBuilder;

namespace Itmo.Dev.Platform.Kafka.MessagePersistence.Configuration;

internal class KafkaBufferingBuilder :
    IConfigurationStep,
    IConsumerStep,
    IFailureStep
{
    private readonly MessagePersistenceConfiguration.Buffering.IBufferingStepStep _stepSelector;
    private readonly BufferStepOptions _stepOptions;

    public KafkaBufferingBuilder(MessagePersistenceConfiguration.Buffering.IBufferingStepStep stepSelector)
    {
        _stepSelector = stepSelector;
        _stepOptions = new BufferStepOptions();
    }

    public IConsumerStep WithProducerConfiguration(
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

        _stepOptions.Name = topicName;
        _stepOptions.PublisherType = typeof(KafkaBufferingStepPublisher);

        _stepSelector.WithStep(_stepOptions);

        return this;
    }

    public IFailureStep WithConsumerConfiguration(
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

    public IKafkaBufferingBuilder WithFailureBlockingBehaviour()
    {
        _stepOptions.FailureProcessor = new ThrowMessageHandleFailureProcessor();
        return this;
    }
}
