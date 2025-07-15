using Itmo.Dev.Platform.Kafka.Configuration;
using Itmo.Dev.Platform.Kafka.Configuration.Builders;
using Itmo.Dev.Platform.Kafka.Consumer;
using Itmo.Dev.Platform.Kafka.Consumer.Builders;
using Itmo.Dev.Platform.Kafka.Producer;
using Itmo.Dev.Platform.Kafka.Producer.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Kafka.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformKafka(
        this IServiceCollection collection,
        Func<IKafkaConfigurationOptionsSelector, IKafkaConfigurationBuilder> configuration)
    {
        var builder = new KafkaConfigurationBuilder(collection);
        configuration.Invoke(builder);

        return collection;
    }

    public static IKafkaConfigurationBuilder AddConsumer(
        this IKafkaConfigurationBuilder builder,
        Func<IConsumerKeySelector, IConsumerBuilder> configuration)
    {
        builder.Services.AddConsumerInternal(configuration);
        return builder;
    }

    public static IKafkaConfigurationBuilder AddProducer(
        this IKafkaConfigurationBuilder builder,
        Func<IProducerKeySelector, IProducerBuilder> configuration)
    {
        builder.Services.AddProducerInternal(configuration);
        return builder;
    }

    internal static void AddConsumerInternal(
        this IServiceCollection collection,
        Func<IConsumerKeySelector, IConsumerBuilder> configuration)
    {
        var selector = new ConsumerKeySelector(collection);
        IConsumerBuilder consumerBuilder = configuration.Invoke(selector);

        consumerBuilder.Build();
    }

    internal static void AddProducerInternal(
        this IServiceCollection collection,
        Func<IProducerKeySelector, IProducerBuilder> configuration)
    {
        var selector = new ProducerKeySelector(collection);
        configuration.Invoke(selector).Build();
    }
}
