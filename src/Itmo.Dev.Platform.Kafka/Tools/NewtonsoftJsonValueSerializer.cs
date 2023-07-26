using Confluent.Kafka;
using Newtonsoft.Json;
using System.Text;

namespace Itmo.Dev.Platform.Kafka.Tools;

internal class NewtonsoftJsonValueSerializer<T> : ISerializer<T>, IDeserializer<T>
{
    private readonly JsonSerializerSettings? _serializerSettings;

    public NewtonsoftJsonValueSerializer(JsonSerializerSettings? serializerSettings = null)
    {
        _serializerSettings = serializerSettings;
    }

    public byte[] Serialize(T data, SerializationContext context)
    {
        return Encoding.Default.GetBytes(JsonConvert.SerializeObject(data, _serializerSettings));
    }

    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (isNull)
        {
            string message = $"Error deserializing protobuf message of Type = {typeof(T)}, null value found";
            throw new ArgumentNullException(nameof(data), message);
        }

        var serialized = Encoding.Default.GetString(data);
        var value = JsonConvert.DeserializeObject<T>(serialized, _serializerSettings);

        if (value is null)
        {
            string message = $"Error deserializing protobuf message of Type = {typeof(T)}, null value found";
            throw new ArgumentNullException(nameof(data), message);
        }

        return value;
    }
}