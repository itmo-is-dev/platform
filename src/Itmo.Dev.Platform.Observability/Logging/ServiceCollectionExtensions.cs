namespace Itmo.Dev.Platform.Observability.Logging;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLoggingPlugins(this IServiceCollection collection)
    {
        collection.AddSingleton<IObservabilityConfigurationPlugin, SerilogConfigurationPlugin>();

        collection
            .AddOptions<PlatformLoggingOptions>()
            .BindConfiguration("Platform:Observability:Logging");

        return collection;
    }
}