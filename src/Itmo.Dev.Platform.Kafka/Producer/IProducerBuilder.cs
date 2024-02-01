using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace Itmo.Dev.Platform.Kafka.Producer;

public interface IProducerKeySelector
{
    IProducerValueSelector<TKey> WithKey<TKey>();
}

public interface IProducerValueSelector<TKey>
{
    IProducerConfigurationSelector<TKey, TValue> WithValue<TValue>();
}

public interface IProducerConfigurationSelector<TKey, TValue>
{
    IProducerKeySerializerSelector<TKey, TValue> WithConfiguration(
        IConfiguration configuration,
        Action<KafkaProducerOptions>? action = null);
}

public interface IProducerKeySerializerSelector<TKey, TValue>
{
    IProducerValueSerializerSelector<TKey, TValue> SerializeKeyWith<T>() where T : class, ISerializer<TKey>;

    IProducerValueSerializerSelector<TKey, TValue> SerializeKeyWith(ISerializer<TKey> serializer);

    IProducerValueSerializerSelector<TKey, TValue> SerializeByDefault();
}

public interface IProducerValueSerializerSelector<TKey, TValue>
{
    IOutboxProducerBuilder SerializeValueWith<T>() where T : class, ISerializer<TValue>;

    IOutboxProducerBuilder SerializeValueWith(ISerializer<TValue> serializer);

    IOutboxProducerBuilder SerializeValueByDefault();
}

public interface IOutboxProducerBuilder : IProducerBuilder
{
    IProducerBuilder WithOutbox();
}

public interface IProducerBuilder
{
    void Build();
}