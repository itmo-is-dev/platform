using System.Data.Common;

namespace Itmo.Dev.Platform.Postgres.Extensions;

public static class DbDataReaderExtensions
{
    public static string? GetNullableString(this DbDataReader reader, int ordinal)
        => reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
}