using Itmo.Dev.Platform.Observability.HealthChecks.Plugins;

namespace Itmo.Dev.Platform.Observability.HealthChecks;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHealthCheckPlugins(this IServiceCollection collection)
    {
        collection.AddSingleton<IObservabilityConfigurationPlugin, HealthChecksConfigurationPlugin>();
        collection.AddSingleton<IObservabilityApplicationPlugin, HealthCheckApplicationPlugin>();

        collection
            .AddOptions<PlatformHealthCheckOptions>()
            .BindConfiguration("Platform:Observability:HealthChecks");

        return collection;
    }
}