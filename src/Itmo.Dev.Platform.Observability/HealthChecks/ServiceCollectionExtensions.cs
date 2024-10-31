using Itmo.Dev.Platform.Observability.HealthChecks.Plugins;
using Itmo.Dev.Platform.Observability.Logging;

namespace Itmo.Dev.Platform.Observability.HealthChecks;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHealthCheckPlugins(this IServiceCollection collection)
    {
        collection.AddSingleton<IObservabilityConfigurationPlugin, HealthChecksConfigurationPlugin>();
        collection.AddSingleton<IObservabilityApplicationPlugin, HealthCheckApplicationPlugin>();
        collection.AddSingleton<ISerilogConfigurationPlugin, HealthChecksSerilogPlugin>();

        collection
            .AddOptions<PlatformHealthCheckOptions>()
            .BindConfiguration("Platform:Observability:HealthChecks");

        return collection;
    }
}