namespace Itmo.Dev.Platform.Observability.Metrics;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMetricsPlugins(this IServiceCollection collection)
    {
        collection.AddSingleton<IObservabilityConfigurationPlugin, MetricsConfigurationPlugin>();
        collection.AddSingleton<IObservabilityApplicationPlugin, MetricsApplicationPlugin>();

        collection
            .AddOptions<PlatformMetricsOptions>()
            .BindConfiguration("Platform:Observability:Metrics");

        return collection;
    }
}