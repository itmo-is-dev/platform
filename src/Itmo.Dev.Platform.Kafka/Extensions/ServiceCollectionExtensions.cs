using Itmo.Dev.Platform.Kafka.Configuration;
using Itmo.Dev.Platform.Kafka.Consumer.Builders;
using Itmo.Dev.Platform.Kafka.Producer.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Kafka.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafka(
        this IServiceCollection collection,
        Func<IKafkaConfigurationOptionsSelector, IKafkaConfigurationBuilder> configuration)
    {
        var builder = new KafkaConfigurationBuilder(collection);
        configuration.Invoke(builder);

        return collection;
    }
    
    public static IKafkaConfigurationBuilder AddConsumer<TKey, TValue>(
        this IKafkaConfigurationBuilder builder,
        Func<IConsumerHandlerSelector<TKey, TValue>, IConsumerBuilder> configuration)
    {
        IConsumerHandlerSelector<TKey, TValue> consumerHandlerSelector = new ConsumerBuilder<TKey, TValue>();
        IConsumerBuilder consumerBuilder = configuration.Invoke(consumerHandlerSelector);

        consumerBuilder.Add(builder.Services);
        return builder;
    }

    public static IKafkaConfigurationBuilder AddProducer<TKey, TValue>(
        this IKafkaConfigurationBuilder builder,
        Func<IProducerKeySerializerSelector<TKey, TValue>, IProducerBuilder> configuration)
    {
        IProducerKeySerializerSelector<TKey, TValue> serializerSelector = new ProducerBuilder<TKey, TValue>();
        IProducerBuilder producerBuilder = configuration.Invoke(serializerSelector);

        producerBuilder.Add(builder.Services);
        return builder;
    }
}