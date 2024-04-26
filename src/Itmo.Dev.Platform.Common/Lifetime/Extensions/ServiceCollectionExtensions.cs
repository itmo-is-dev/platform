using Itmo.Dev.Platform.Common.Features;
using Itmo.Dev.Platform.Common.Lifetime.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Itmo.Dev.Platform.Common.Lifetime.Extensions;

public static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddPlatformLifetimes(this IServiceCollection collection)
    {
        collection.AddPlatformFeature<PlatformLifetimeFeature>();

        collection.AddSingleton<IPlatformLifetime, PlatformLifetime>();
        collection.AddHostedService<PlatformLifetimeInitializerService>();

        return collection;
    }

    public static IServiceCollection AddPlatformLifetimeInitializer<TInitializer>(this IServiceCollection collection)
        where TInitializer : class, IPlatformLifetimeInitializer
    {
        collection.CheckPlatformFeature<PlatformLifetimeFeature>();
        collection.TryAddEnumerable(ServiceDescriptor.Singleton<IPlatformLifetimeInitializer, TInitializer>());

        return collection;
    }

    public static IServiceCollection AddPlatformLifetimePostInitializer<TInitializer>(
        this IServiceCollection collection)
        where TInitializer : class, IPlatformLifetimePostInitializer
    {
        collection.CheckPlatformFeature<PlatformLifetimeFeature>();
        collection.TryAddEnumerable(ServiceDescriptor.Singleton<IPlatformLifetimePostInitializer, TInitializer>());

        return collection;
    }
}