using Confluent.Kafka;
using Itmo.Dev.Platform.Kafka.Producer.Models;
using Itmo.Dev.Platform.Kafka.Producer.Services;
using Itmo.Dev.Platform.Kafka.QualifiedServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

    public IProducerValueSerializerSelector<TKey, TValue> SerializeKeyWith(ISerializer<TKey> serializer)
    {
        var action = _action;

        _action = collection =>
        {
            action(collection);
            return collection.AddSingleton(serializer);
        };

        return this;
    }

    public IProducerValueSerializerSelector<TKey, TValue> SerializeByDefault()
    {
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

    public IProducerConfigurationSelector<TKey, TValue> SerializeValueWith(ISerializer<TValue> serializer)
    {
        var action = _action;

        _action = collection =>
        {
            action(collection);
            return collection.AddSingleton(serializer);
        };

        return this;
    }

    public IProducerConfigurationSelector<TKey, TValue> SerializeValueByDefault()
    {
        return this;
    }

    public IProducerBuilder UseConfiguration<T>() where T : class, IKafkaProducerConfiguration
    {
        var action = _action;

        _action = collection =>
        {
            action(collection);

            var s = new OptionsQualifiedService<TKey, TValue, T>();
            return collection.AddSingleton<IKeyValueQualifiedService<TKey, TValue, IKafkaProducerConfiguration>>(s);
        };

        return this;
    }

    public IProducerBuilder UseNamedOptionsConfiguration(
        string name,
        IConfiguration configuration,
        Action<IKafkaProducerConfiguration>? postConfigure = null)
    {
        var action = _action;

        _action = collection =>
        {
            action(collection);

            collection.Configure<KafkaProducerConfiguration>(name, configuration);

            if (postConfigure is not null)
            {
                collection.PostConfigure<KafkaProducerConfiguration>(name, postConfigure);
            }

            var s = new NamedOptionsQualifiedService<TKey, TValue, KafkaProducerConfiguration>(name);
            return collection.AddSingleton<IKeyValueQualifiedService<TKey, TValue, IKafkaProducerConfiguration>>(s);
        };

        return this;
    }

    public void Add(IServiceCollection collection)
    {
        _action(collection);
        collection.AddScoped<IKafkaMessageProducer<TKey, TValue>, KafkaMessageProducer<TKey, TValue>>();
    }
}