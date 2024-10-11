namespace Itmo.Dev.Platform.Observability.Tracing;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTracingPlugins(this IServiceCollection collection)
    {
        collection.AddSingleton<IObservabilityConfigurationPlugin, TracingObservabilityPlugin>();

        collection
            .AddOptions<PlatformObservabilityTracingOptions>()
            .BindConfiguration("Platform:Observability:Tracing");

        return collection;
    }
}