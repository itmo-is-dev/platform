using Itmo.Dev.Platform.Observability.HealthChecks;
using Itmo.Dev.Platform.Observability.Logging;
using Itmo.Dev.Platform.Observability.Tracing;

namespace Itmo.Dev.Platform.Observability.Sentry;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSentryPlugins(this IServiceCollection collection)
    {
        collection.AddSingleton<IObservabilityConfigurationPlugin, SentryConfigurationPlugin>();
        collection.AddSingleton<ITracingConfigurationPlugin, SentryTracingPlugin>();
        collection.AddSingleton<ISerilogConfigurationPlugin, SentrySerilogPlugin>();
        collection.AddSingleton<IHealthCheckConfigurationPlugin, SentryHealthChecksPlugin>();

        collection
            .AddOptions<PlatformSentryOptions>()
            .BindConfiguration("Platform:Observability:Sentry");

        return collection;
    }
}