namespace Itmo.Dev.Platform.Observability.Metrics;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMetricsPlugins(this IServiceCollection collection)
    {
        collection.AddSingleton<IObservabilityConfigurationPlugin, MetricsObservabilityConfigurationPlugin>();
        collection.AddSingleton<IObservabilityApplicationPlugin, MetricsObservabilityApplicationPlugin>();

        collection
            .AddOptions<PlatformObservabilityMetricsOptions>()
            .BindConfiguration("Platform:Observability:Metrics");

        return collection;
    }
}