using Newtonsoft.Json;
using System.Data.Common;

namespace Itmo.Dev.Platform.Postgres.Extensions;

public static class DbDataReaderExtensions
{
    public static string? GetNullableString(this DbDataReader reader, int ordinal)
        => reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);

    public static T GetFieldValue<T>(this DbDataReader reader, int ordinal, JsonSerializerSettings serializerSettings)
    {
        var serialized = reader.GetString(ordinal);
        var deserialized = JsonConvert.DeserializeObject<T>(serialized, serializerSettings);

        if (deserialized is null)
            throw new ArgumentException($"Value at position {ordinal} is not a valid {typeof(T)} json");

        return deserialized;
    }
}