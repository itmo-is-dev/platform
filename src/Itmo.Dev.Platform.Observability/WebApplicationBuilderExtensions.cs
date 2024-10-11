using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.Observability.Logging;
using Itmo.Dev.Platform.Observability.Sentry;
using Itmo.Dev.Platform.Observability.Tracing;

namespace Itmo.Dev.Platform.Observability;

public static class WebApplicationBuilderExtensions
{
    public static void AddPlatformObservability(this WebApplicationBuilder builder)
    {
        var collection = new ServiceCollection();

        collection.AddSingleton<IConfiguration>(builder.Configuration);
        collection.AddSingleton<IConfigurationRoot>(builder.Configuration);

        collection.AddPlatform();
        collection.AddLogging(x => x.AddConsole());

        collection.AddSentryPlugins();
        collection.AddTracingPlugins();
        collection.AddLoggingPlugins();

        using var provider = collection.BuildServiceProvider();
        var plugins = provider.GetRequiredService<IEnumerable<IObservabilityConfigurationPlugin>>();

        foreach (IObservabilityConfigurationPlugin plugin in plugins)
        {
            plugin.Configure(builder);
        }
    }
}