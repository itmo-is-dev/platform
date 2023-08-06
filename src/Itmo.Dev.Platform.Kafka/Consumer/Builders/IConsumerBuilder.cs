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
}

public interface IConsumerValueDeserializerSelector<TKey, TValue>
{
    IConsumerConfigurationSelector<TKey, TValue> DeserializeValueWith<T>() where T : class, IDeserializer<TValue>;
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