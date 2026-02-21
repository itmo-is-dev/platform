using Confluent.Kafka;
using Google.Protobuf;

namespace Itmo.Dev.Platform.Kafka.Tools;

public class ProtobufValueSerializer<T> : ISerializer<T>, IDeserializer<T> where T : IMessage<T>, new()
{
    private static readonly MessageParser<T> Parser = new(() => new T());

    public byte[] Serialize(T data, SerializationContext context)
    {
        return data.ToByteArray();
    }

    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (isNull is false)
            return Parser.ParseFrom(data);

        string message = $"Error deserializing protobuf message of Type = {typeof(T)}, null value found";
        throw new ArgumentNullException(nameof(data), message);
    }
}