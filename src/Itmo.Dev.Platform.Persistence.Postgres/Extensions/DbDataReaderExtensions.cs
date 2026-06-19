using Itmo.Dev.Platform.Common.Serialization;
using System.Data;
using System.Data.Common;

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
        IPlatformSerializer serializer)
        where T : notnull
    {
        var serialized = reader.GetString(ordinal);
        var deserialized = serializer.Deserialize<T>(serialized);

        if (deserialized is null)
            throw new ArgumentException($"Value at position {ordinal} is not a valid {typeof(T)} json");

        return deserialized;
    }

    public static T GetJsonFieldValue<T>(
        this DbDataReader reader,
        string name,
        IPlatformSerializer serializer)
        where T : notnull
    {
        return reader.GetJsonFieldValue<T>(reader.GetOrdinal(name), serializer);
    }

    public static T[] GetJsonArrayFieldValue<T>(
        this DbDataReader reader,
        int ordinal,
        IPlatformSerializer serializer)
        where T : notnull
    {
        var serialized = reader.GetFieldValue<string[]>(ordinal);

        return Deserialize(serialized, serializer).ToArray();

        static IEnumerable<T> Deserialize(string[] serialized, IPlatformSerializer serializer)
        {
            foreach (string s in serialized)
            {
                var deserialized = serializer.Deserialize<T>(s);

                if (deserialized is null)
                    throw new ArgumentException($"Invalid json for type {typeof(T)} found");

                yield return deserialized;
            }
        }
    }

    public static T[] GetJsonArrayFieldValue<T>(
        this DbDataReader reader,
        string name,
        IPlatformSerializer serializer)
        where T : notnull
    {
        return reader.GetJsonArrayFieldValue<T>(reader.GetOrdinal(name), serializer);
    }
}
