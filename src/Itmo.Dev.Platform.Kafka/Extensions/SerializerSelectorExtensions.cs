using Google.Protobuf;
using Itmo.Dev.Platform.Kafka.Producer.Builders;
using Itmo.Dev.Platform.Kafka.Tools;

namespace Itmo.Dev.Platform.Kafka.Extensions;

public static class SerializerSelectorExtensions
{
    public static IProducerValueSerializerSelector<TKey, TValue> SerializeKeyWithProto<TKey, TValue>(
        this IProducerKeySerializerSelector<TKey, TValue> selector)
        where TKey : IMessage<TKey>, new()
    {
        return selector.SerializeKeyWith<ProtobufValueSerializer<TKey>>();
    }

    public static IProducerConfigurationSelector<TKey, TValue> SerializeValueWithProto<TKey, TValue>(
        this IProducerValueSerializerSelector<TKey, TValue> selector)
        where TValue : IMessage<TValue>, new()
    {
        return selector.SerializeValueWith<ProtobufValueSerializer<TValue>>();
    }

    public static IProducerValueSerializerSelector<TKey, TValue> SerializeKeyWithNewtonsoft<TKey, TValue>(
        this IProducerKeySerializerSelector<TKey, TValue> selector)
    {
        return selector.SerializeKeyWith<NewtonsoftJsonValueSerializer<TKey>>();
    }

    public static IProducerConfigurationSelector<TKey, TValue> SerializeValueWithNewtonsoft<TKey, TValue>(
        this IProducerValueSerializerSelector<TKey, TValue> selector)
    {
        return selector.SerializeValueWith<NewtonsoftJsonValueSerializer<TValue>>();
    }
}