namespace Itmo.Dev.Platform.BackgroundTasks.Extensions;

internal static class LazyExtensions
{
    public static Lazy<T> RecreateIfComputed<T>(this Lazy<T> lazy, Func<T> factory)
    {
        return lazy.IsValueCreated ? new Lazy<T>(factory) : lazy;
    }
}