namespace Itmo.Dev.Platform.Observability.Tracing;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTracingPlugins(this IServiceCollection collection)
    {
        collection.AddSingleton<IObservabilityConfigurationPlugin, TracingConfigurationPlugin>();

        collection
            .AddOptions<PlatformTracingOptions>()
            .BindConfiguration("Platform:Observability:Tracing");

        return collection;
    }
}