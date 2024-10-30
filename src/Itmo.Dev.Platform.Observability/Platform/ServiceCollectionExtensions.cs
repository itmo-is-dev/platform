using Itmo.Dev.Platform.Observability.HealthChecks;

namespace Itmo.Dev.Platform.Observability.Platform;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformPlugins(this IServiceCollection collection)
    {
        collection.AddSingleton<IHealthCheckConfigurationPlugin, PlatformHealthCheckPlugin>();
        return collection;
    }
}