using Confluent.Kafka;
using Itmo.Dev.Platform.Kafka.Producer.Services;
using Itmo.Dev.Platform.Kafka.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Kafka.Producer.Builders;

internal class ProducerBuilder<TKey, TValue> :
    IProducerKeySerializerSelector<TKey, TValue>,
    IProducerValueSerializerSelector<TKey, TValue>,
    IProducerConfigurationSelector<TKey, TValue>,
    IProducerBuilder
{
    private Func<IServiceCollection, IServiceCollection> _action;

    public ProducerBuilder()
    {
        _action = _ => _;
    }

    public IProducerValueSerializerSelector<TKey, TValue> SerializeKeyWith<T>() where T : class, ISerializer<TKey>
    {
        var action = _action;

        _action = collection =>
        {
            action(collection);
            return collection.AddSingleton<ISerializer<TKey>, T>();
        };

        return this;
    }

    public IProducerConfigurationSelector<TKey, TValue> SerializeValueWith<T>() where T : class, ISerializer<TValue>
    {
        var action = _action;

        _action = collection =>
        {
            action(collection);
            return collection.AddSingleton<ISerializer<TValue>, T>();
        };

        return this;
    }

    public IProducerBuilder UseConfiguration<T>() where T : class, IKafkaProducerConfiguration
    {
        var action = _action;

        _action = collection =>
        {
            action(collection);

            return collection
                .AddSingleton(new KeyValueQualifiedService<TKey, TValue, IOptions<IKafkaProducerConfiguration>>(
                    typeof(IOptions<T>)))
                .AddSingleton(new KeyValueQualifiedService<TKey, TValue, IOptionsSnapshot<IKafkaProducerConfiguration>>(
                    typeof(IOptionsSnapshot<T>)))
                .AddSingleton(new KeyValueQualifiedService<TKey, TValue, IOptionsMonitor<IKafkaProducerConfiguration>>(
                    typeof(IOptionsMonitor<T>)));
        };

        return this;
    }

    public void Add(IServiceCollection collection)
    {
        _action(collection);
        collection.AddScoped<IKafkaMessageProducer<TKey, TValue>, KafkaMessageProducer<TKey, TValue>>();
    }
}