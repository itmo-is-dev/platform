using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.Observability.Extensibility;
using Itmo.Dev.Platform.Observability.Logging;
using Itmo.Dev.Platform.Observability.Metrics;
using Itmo.Dev.Platform.Observability.Sentry;
using Itmo.Dev.Platform.Observability.Tracing;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Itmo.Dev.Platform.Observability;

public static class WebApplicationBuilderExtensions
{
    public static void AddPlatformObservability(
        this WebApplicationBuilder builder,
        Action<IPlatformObservabilityExtensionConfigurator>? configuration)
    {
        var collection = new ServiceCollection();

        collection.AddSingleton<IConfiguration>(builder.Configuration);
        collection.AddSingleton<IConfigurationRoot>(builder.Configuration);

        if (configuration is not null)
        {
            var configurator = new PlatformObservabilityExtensionConfigurator(collection);
            configuration.Invoke(configurator);
        }

        collection.AddPlatform();
        collection.AddLogging(x => x.AddConsole());

        collection.AddSentryPlugins();
        collection.AddTracingPlugins();
        collection.AddLoggingPlugins();
        collection.AddMetricsPlugins();

        using var provider = collection.BuildServiceProvider();
        var plugins = provider.GetRequiredService<IEnumerable<IObservabilityConfigurationPlugin>>();

        foreach (IObservabilityConfigurationPlugin plugin in plugins)
        {
            plugin.Configure(builder);
        }

        IEnumerable<IObservabilityApplicationPlugin> applicationPlugins = provider
            .GetRequiredService<IEnumerable<IObservabilityApplicationPlugin>>();

        foreach (IObservabilityApplicationPlugin applicationPlugin in applicationPlugins)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton(applicationPlugin));
        }
    }
}