using Confluent.Kafka;
using Itmo.Dev.Platform.Kafka.Consumer.Services;
using Itmo.Dev.Platform.Kafka.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Kafka.Consumer.Builders;

internal class ConsumerBuilder<TKey, TValue> :
    IConsumerHandlerSelector<TKey, TValue>,
    IConsumerKeyDeserializerSelector<TKey, TValue>,
    IConsumerValueDeserializerSelector<TKey, TValue>,
    IConsumerConfigurationSelector<TKey, TValue>,
    IConsumerBuilder
{
    private Func<IServiceCollection, IServiceCollection> _action;

    public ConsumerBuilder()
    {
        _action = _ => _;
    }

    public IConsumerKeyDeserializerSelector<TKey, TValue> HandleWith<T>()
        where T : class, IKafkaMessageHandler<TKey, TValue>
    {
        var action = _action;

        _action = collection =>
        {
            action(collection);

            collection.TryAddScoped<T>();

            collection.AddSingleton(
                new KeyValueQualifiedService<TKey, TValue, IKafkaMessageHandler<TKey, TValue>>(typeof(T)));

            return collection;
        };

        return this;
    }

    public IConsumerValueDeserializerSelector<TKey, TValue> DeserializeKeyWith<T>() where T : class, IDeserializer<TKey>
    {
        var action = _action;

        _action = collection =>
        {
            action(collection);
            return collection.AddSingleton<IDeserializer<TKey>, T>();
        };

        return this;
    }

    public IConsumerConfigurationSelector<TKey, TValue> DeserializeValueWith<T>() where T : class, IDeserializer<TValue>
    {
        var action = _action;

        _action = collection =>
        {
            action(collection);
            return collection.AddSingleton<IDeserializer<TValue>, T>();
        };

        return this;
    }

    public IConsumerBuilder UseConfiguration<T>() where T : class, IKafkaConsumerConfiguration
    {
        var action = _action;

        _action = collection =>
        {
            action(collection);

            return collection
                .AddSingleton(new KeyValueQualifiedService<TKey, TValue, IOptions<IKafkaConsumerConfiguration>>(
                    typeof(IOptions<T>)))
                .AddSingleton(new KeyValueQualifiedService<TKey, TValue, IOptionsSnapshot<IKafkaConsumerConfiguration>>(
                    typeof(IOptionsSnapshot<T>)))
                .AddSingleton(new KeyValueQualifiedService<TKey, TValue, IOptionsMonitor<IKafkaConsumerConfiguration>>(
                    typeof(IOptionsMonitor<T>)));
        };

        return this;
    }

    public void Add(IServiceCollection collection)
    {
        _action.Invoke(collection);
        collection.AddHostedService<KafkaConsumerService<TKey, TValue>>();
    }
}