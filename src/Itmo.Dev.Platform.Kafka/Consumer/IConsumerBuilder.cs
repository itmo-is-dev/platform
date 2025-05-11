using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace Itmo.Dev.Platform.Kafka.Consumer;

public interface IConsumerKeySelector
{
    IConsumerValueSelector<TKey> WithKey<TKey>();
}

public interface IConsumerValueSelector<TKey>
{
    IConsumerConfigurationSelector<TKey, TValue> WithValue<TValue>();
}

public interface IConsumerConfigurationSelector<TKey, TValue>
{
    IConsumerKeyDeserializerSelector<TKey, TValue> WithConfiguration(
        IConfiguration configuration,
        Action<KafkaConsumerOptions>? action = null);
}

public interface IConsumerKeyDeserializerSelector<TKey, TValue>
{
    IConsumerValueDeserializerSelector<TKey, TValue> DeserializeKeyWith<T>()
        where T : class, IDeserializer<TKey>;

    IConsumerValueDeserializerSelector<TKey, TValue> DeserializeKeyWith(IDeserializer<TKey> deserializer);

    IConsumerValueDeserializerSelector<TKey, TValue> DeserializeKeyByDefault();
}

public interface IConsumerValueDeserializerSelector<out TKey, TValue>
{
    IConsumerHandlerSelector<TKey, TValue> DeserializeValueWith<T>()
        where T : class, IDeserializer<TValue>;

    IConsumerHandlerSelector<TKey, TValue> DeserializeValueWith(IDeserializer<TValue> deserializer);

    IConsumerHandlerSelector<TKey, TValue> DeserializeValueByDefault();
}

public interface IConsumerHandlerSelector<out TKey, out TValue>
{
    IConsumerBuilder HandleWith<T>()
        where T : class, IKafkaConsumerHandler<TKey, TValue>;

    IConsumerBuilder HandleInboxWith<T>()
        where T : class, IKafkaInboxHandler<TKey, TValue>;
}

public interface IConsumerBuilder
{
    void Build();
}
