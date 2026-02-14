using Itmo.Dev.Platform.Kafka.MessagePersistence.Configuration;

// ReSharper disable once CheckNamespace
namespace Itmo.Dev.Platform.MessagePersistence;

public static class BufferingConfigurationExtensions
{
    public static MessagePersistenceConfiguration.Buffering.IBufferingStepStep AddKafkaBufferingStep(
        this MessagePersistenceConfiguration.Buffering.IBufferingStepStep selector,
        Func<IKafkaBufferingBuilder.IConfigurationStep, IKafkaBufferingBuilder> configuration)
    {
        var builder = new KafkaBufferingBuilder(selector);
        configuration.Invoke(builder);

        return selector;
    }
}
