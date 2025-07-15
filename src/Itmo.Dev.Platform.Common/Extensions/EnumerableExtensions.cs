// ReSharper disable once CheckNamespace

namespace System.Collections.Generic;

public static partial class EnumerableExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable)
    {
        return enumerable
            .Where(x => x is not null)
            .Cast<T>();
    }
}
