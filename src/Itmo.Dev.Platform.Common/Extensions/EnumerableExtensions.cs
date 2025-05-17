namespace Itmo.Dev.Platform.Common.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable)
    {
        return enumerable
            .Where(x => x is not null)
            .Cast<T>();
    }
}
