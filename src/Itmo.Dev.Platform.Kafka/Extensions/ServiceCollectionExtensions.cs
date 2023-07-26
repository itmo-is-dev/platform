using Itmo.Dev.Platform.Kafka.Consumer.Builders;
using Itmo.Dev.Platform.Kafka.Producer.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Kafka.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafkaConsumer<TKey, TValue>(
        this IServiceCollection collection,
        Func<IConsumerHandlerSelector<TKey, TValue>, IConsumerBuilder> configuration)
    {
        IConsumerHandlerSelector<TKey, TValue> consumerHandlerSelector = new ConsumerBuilder<TKey, TValue>();
        IConsumerBuilder builder = configuration.Invoke(consumerHandlerSelector);

        builder.Add(collection);
        return collection;
    }

    public static IServiceCollection AddKafkaProducer<TKey, TValue>(
        this IServiceCollection collection,
        Func<IProducerKeySerializerSelector<TKey, TValue>, IProducerBuilder> configuration)
    {
        IProducerKeySerializerSelector<TKey, TValue> serializerSelector = new ProducerBuilder<TKey, TValue>();
        IProducerBuilder builder = configuration.Invoke(serializerSelector);

        builder.Add(collection);
        return collection;
    }
}