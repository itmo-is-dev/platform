using Itmo.Dev.Platform.Common.Options;
using Microsoft.Extensions.Options;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Itmo.Dev.Platform.Observability.Tracing;

internal class TracingObservabilityPlugin : IObservabilityConfigurationPlugin
{
    private readonly PlatformObservabilityTracingOptions _options;
    private readonly PlatformOptions _platformOptions;
    private readonly IEnumerable<ITracingConfigurationPlugin> _plugins;
    private readonly ILogger<TracingObservabilityPlugin> _logger;

    public TracingObservabilityPlugin(
        IOptions<PlatformObservabilityTracingOptions> options,
        IOptions<PlatformOptions> platformOptions,
        IEnumerable<ITracingConfigurationPlugin> plugins,
        ILogger<TracingObservabilityPlugin> logger)
    {
        _plugins = plugins;
        _logger = logger;
        _options = options.Value;
        _platformOptions = platformOptions.Value;
    }

    public void Configure(WebApplicationBuilder builder)
    {
        if (_options.IsEnabled is false)
        {
            _logger.LogInformation("OpenTelemetry tracing is disabled");
            return;
        }

        builder.Services
            .AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .ConfigureResource(x => x.AddService(_platformOptions.ServiceName))
                    .SetSampler(new AlwaysOnSampler())
                    .AddAspNetCoreInstrumentation(x => x.RecordException = true)
                    .AddGrpcCoreInstrumentation();

                foreach (string source in _options.Sources ?? [])
                {
                    tracing.AddSource(source);
                }

                foreach (ITracingConfigurationPlugin plugin in _plugins)
                {
                    plugin.Configure(builder, tracing);
                }
            });

        _logger.LogInformation("OpenTelemetry tracing initialized");
    }
}