using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Common.Features;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformFeature<TFeature>(this IServiceCollection collection)
        where TFeature : class, IPlatformFeature
    {
        return collection.AddSingleton<TFeature>();
    }

    public static bool HasPlatformFeature<TFeature>(this IServiceCollection collection)
        where TFeature : IPlatformFeature
    {
        return collection.Any(x => x.ServiceType == typeof(TFeature));
    }

    public static void CheckPlatformFeature<TFeature>(this IServiceCollection collection)
        where TFeature : class, IPlatformFeature
    {
        if (HasPlatformFeature<TFeature>(collection) is false)
            throw PlatformFeatureMissingException.Create<TFeature>();
    }
}