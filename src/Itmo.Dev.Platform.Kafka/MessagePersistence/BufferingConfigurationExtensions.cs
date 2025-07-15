using Itmo.Dev.Platform.Kafka.MessagePersistence.Configuration;

// ReSharper disable once CheckNamespace
namespace Itmo.Dev.Platform.MessagePersistence;

public static class BufferingConfigurationExtensions
{
    public static IMessagePersistenceBufferingStepSelector AddKafkaBufferingStep(
        this IMessagePersistenceBufferingStepSelector selector,
        Func<IKafkaBufferingProducerConfigurationSelector, IKafkaBufferingBuilder> configuration)
    {
        var builder = new KafkaBufferingBuilder(selector);
        configuration.Invoke(builder);

        return selector;
    }
}
