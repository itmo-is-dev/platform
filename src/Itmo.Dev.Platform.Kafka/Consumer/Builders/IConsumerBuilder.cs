using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Kafka.Consumer.Builders;

public interface IConsumerHandlerSelector<TKey, TValue>
{
    IConsumerKeyDeserializerSelector<TKey, TValue> HandleWith<T>() where T : class, IKafkaMessageHandler<TKey, TValue>;
}

public interface IConsumerKeyDeserializerSelector<TKey, TValue>
{
    IConsumerValueDeserializerSelector<TKey, TValue> DeserializeKeyWith<T>() where T : class, IDeserializer<TKey>;

    IConsumerValueDeserializerSelector<TKey, TValue> DeserializeKeyWith(IDeserializer<TKey> deserializer);

    IConsumerValueDeserializerSelector<TKey, TValue> DeserializeKeyByDefault();
}

public interface IConsumerValueDeserializerSelector<TKey, TValue>
{
    IConsumerConfigurationSelector<TKey, TValue> DeserializeValueWith<T>() where T : class, IDeserializer<TValue>;

    IConsumerConfigurationSelector<TKey, TValue> DeserializeValueWith(IDeserializer<TValue> deserializer);

    IConsumerConfigurationSelector<TKey, TValue> DeserializeValueByDefault();
}

public interface IConsumerConfigurationSelector<TKey, TValue>
{
    IConsumerBuilder UseConfiguration<T>() where T : class, IKafkaConsumerConfiguration;

    IConsumerBuilder UseNamedOptionsConfiguration(string name, IConfiguration configuration);
}

public interface IConsumerBuilder
{
    void Add(IServiceCollection collection);
}