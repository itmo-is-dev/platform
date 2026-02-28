using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using Type = System.Type;

namespace Itmo.Dev.Platform.Testing.Behavioural.Tools.Converters;

public sealed class TimestampConverter : JsonConverter<Timestamp>
{
    public override void WriteJson(JsonWriter writer, Timestamp? value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.ToDateTimeOffset());
    }

    public override Timestamp? ReadJson(
        JsonReader reader,
        Type objectType,
        Timestamp? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        return reader.ReadAsDateTimeOffset()?.ToTimestamp();
    }
}
