using Google.Protobuf;
using Itmo.Dev.Platform.Kafka.Consumer.Builders;
using Itmo.Dev.Platform.Kafka.Tools;

namespace Itmo.Dev.Platform.Kafka.Extensions;

public static class DeserializerSelectorExtensions
{
    public static IConsumerValueDeserializerSelector<TKey, TValue> DeserializeKeyWithProto<TKey, TValue>(
        this IConsumerKeyDeserializerSelector<TKey, TValue> selector)
        where TKey : IMessage<TKey>, new()
    {
        return selector.DeserializeKeyWith<ProtobufValueSerializer<TKey>>();
    }
    
    public static IConsumerConfigurationSelector<TKey, TValue> DeserializeValueWithProto<TKey, TValue>(
        this IConsumerValueDeserializerSelector<TKey, TValue> selector)
        where TValue : IMessage<TValue>, new()
    {
        return selector.DeserializeValueWith<ProtobufValueSerializer<TValue>>();
    }
    
    public static IConsumerValueDeserializerSelector<TKey, TValue> DeserializeKeyWithNewtonsoft<TKey, TValue>(
        this IConsumerKeyDeserializerSelector<TKey, TValue> selector)
    {
        return selector.DeserializeKeyWith<NewtonsoftJsonValueSerializer<TKey>>();
    }
    
    public static IConsumerConfigurationSelector<TKey, TValue> DeserializeValueWithNewtonsoft<TKey, TValue>(
        this IConsumerValueDeserializerSelector<TKey, TValue> selector)
    {
        return selector.DeserializeValueWith<NewtonsoftJsonValueSerializer<TValue>>();
    }
}