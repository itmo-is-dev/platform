using Confluent.Kafka;
using Itmo.Dev.Platform.Kafka.Consumer.Models;
using Itmo.Dev.Platform.Kafka.Consumer.Services;
using Itmo.Dev.Platform.Kafka.QualifiedServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

            collection.AddSingleton<IKeyValueQualifiedService<TKey, TValue, IKafkaMessageHandler<TKey, TValue>>>(
                new TypeKeyValueQualifiedService<TKey, TValue, IKafkaMessageHandler<TKey, TValue>>(typeof(T)));

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

    public IConsumerValueDeserializerSelector<TKey, TValue> DeserializeKeyWith(IDeserializer<TKey> deserializer)
    {
        var action = _action;

        _action = collection =>
        {
            action(collection);
            return collection.AddSingleton(deserializer);
        };

        return this;
    }

    public IConsumerValueDeserializerSelector<TKey, TValue> DeserializeKeyByDefault()
    {
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

    public IConsumerConfigurationSelector<TKey, TValue> DeserializeValueWith(IDeserializer<TValue> deserializer)
    {
        var action = _action;

        _action = collection =>
        {
            action(collection);

            return collection.AddSingleton(deserializer);
        };

        return this;
    }

    public IConsumerConfigurationSelector<TKey, TValue> DeserializeValueByDefault()
    {
        return this;
    }

    public IConsumerBuilder UseConfiguration<T>() where T : class, IKafkaConsumerConfiguration
    {
        var action = _action;

        _action = collection =>
        {
            action(collection);

            var s = new OptionsQualifiedService<TKey, TValue, T>();
            return collection.AddSingleton<IKeyValueQualifiedService<TKey, TValue, IKafkaConsumerConfiguration>>(s);
        };

        return this;
    }

    public IConsumerBuilder UseNamedOptionsConfiguration(
        string name,
        IConfiguration configuration,
        Action<IKafkaConsumerConfiguration>? postConfigure)
    {
        var action = _action;

        _action = collection =>
        {
            action(collection);

            collection.Configure<KafkaConsumerConfiguration>(name, configuration);

            if (postConfigure is not null)
            {
                collection.PostConfigure<KafkaConsumerConfiguration>(name, postConfigure);
            }

            var s = new NamedOptionsQualifiedService<TKey, TValue, KafkaConsumerConfiguration>(name);
            return collection.AddSingleton<IKeyValueQualifiedService<TKey, TValue, IKafkaConsumerConfiguration>>(s);
        };

        return this;
    }

    public void Add(IServiceCollection collection)
    {
        _action.Invoke(collection);
        collection.AddHostedService<KafkaConsumerService<TKey, TValue>>();
    }
}