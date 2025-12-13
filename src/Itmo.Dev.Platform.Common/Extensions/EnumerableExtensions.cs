// ReSharper disable once CheckNamespace

namespace System.Collections.Generic;

public static partial class EnumerableExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable)
        where T : class
    {
        return enumerable
            .Where(x => x is not null)
            .Cast<T>();
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable)
        where T : struct
    {
        return enumerable
            .Where(x => x is not null)
            .Cast<T>();
    }
}
