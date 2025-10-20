using Newtonsoft.Json;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Itmo.Dev.Platform.Persistence.Postgres.Extensions;

public static class DbDataReaderExtensions
{
    public static string? GetNullableString(this DbDataReader reader, int ordinal)
        => reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);

    public static string? GetNullableString(this DbDataReader reader, string name)
        => reader.GetNullableString(reader.GetOrdinal(name));

    public static T[]? GetNullableArrayValue<T>(this DbDataReader reader, int ordinal)
        => reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<T[]>(ordinal);

    public static T[]? GetNullableArrayValue<T>(this DbDataReader reader, string name)
        => reader.IsDBNull(name) ? null : reader.GetFieldValue<T[]>(name);

    public static T GetJsonFieldValue<T>(
        this DbDataReader reader,
        int ordinal,
        JsonSerializerSettings serializerSettings)
        where T : notnull
    {
        var serialized = reader.GetString(ordinal);
        var deserialized = JsonConvert.DeserializeObject<T>(serialized, serializerSettings);

        if (deserialized is null)
            throw new ArgumentException($"Value at position {ordinal} is not a valid {typeof(T)} json");

        return deserialized;
    }

    public static T GetJsonFieldValue<T>(
        this DbDataReader reader,
        string name,
        JsonSerializerSettings serializerSettings)
        where T : notnull
    {
        var serialized = reader.GetString(name);
        var deserialized = JsonConvert.DeserializeObject<T>(serialized, serializerSettings);

        if (deserialized is null)
            throw new ArgumentException($"Value at position {name} is not a valid {typeof(T)} json");

        return deserialized;
    }

    public static T[] GetJsonArrayFieldValue<T>(
        this DbDataReader reader,
        int ordinal,
        JsonSerializerSettings serializerSettings)
        where T : notnull
    {
        var serialized = reader.GetFieldValue<string[]>(ordinal);

        return Deserialize(serialized, serializerSettings).ToArray();

        static IEnumerable<T> Deserialize(string[] serialized, JsonSerializerSettings serializerSettings)
        {
            serializerSettings = new JsonSerializerSettings(serializerSettings)
            {
                StringEscapeHandling = StringEscapeHandling.EscapeHtml,
            };

            foreach (string s in serialized)
            {
                var deserialized = JsonConvert.DeserializeObject<T>(s, serializerSettings);

                if (deserialized is null)
                    throw new ArgumentException($"Invalid json for type {typeof(T)} found");

                yield return deserialized;
            }
        }
    }

    public static T[] GetJsonArrayFieldValue<T>(
        this DbDataReader reader,
        string name,
        JsonSerializerSettings serializerSettings)
        where T : notnull
    {
        var serialized = reader.GetFieldValue<string[]>(name);

        return Deserialize(serialized, serializerSettings).ToArray();

        static IEnumerable<T> Deserialize(string[] serialized, JsonSerializerSettings serializerSettings)
        {
            serializerSettings = new JsonSerializerSettings(serializerSettings)
            {
                StringEscapeHandling = StringEscapeHandling.EscapeHtml,
            };

            foreach (string s in serialized)
            {
                var deserialized = JsonConvert.DeserializeObject<T>(s, serializerSettings);

                if (deserialized is null)
                    throw new ArgumentException($"Invalid json for type {typeof(T)} found");

                yield return deserialized;
            }
        }
    }
}
