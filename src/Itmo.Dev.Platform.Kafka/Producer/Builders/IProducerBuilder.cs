using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Kafka.Producer.Builders;

public interface IProducerKeySerializerSelector<TKey, TValue>
{
    IProducerValueSerializerSelector<TKey, TValue> SerializeKeyWith<T>() where T : class, ISerializer<TKey>;
}

public interface IProducerValueSerializerSelector<TKey, TValue>
{
    IProducerConfigurationSelector<TKey, TValue> SerializeValueWith<T>() where T : class, ISerializer<TValue>;
}

public interface IProducerConfigurationSelector<TKey, TValue>
{
    IProducerBuilder UseConfiguration<T>() where T : class, IKafkaProducerConfiguration;
}

public interface IProducerBuilder
{
    void Add(IServiceCollection collection);
}