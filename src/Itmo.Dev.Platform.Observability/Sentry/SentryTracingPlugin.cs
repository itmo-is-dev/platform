using Itmo.Dev.Platform.Observability.Tracing;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;
using Sentry.OpenTelemetry;

namespace Itmo.Dev.Platform.Observability.Sentry;

internal class SentryTracingPlugin : ITracingConfigurationPlugin
{
    private readonly PlatformObservabilitySentryOptions _options;
    private readonly ILogger<SentryTracingPlugin> _logger;

    public SentryTracingPlugin(
        IOptions<PlatformObservabilitySentryOptions> options,
        ILogger<SentryTracingPlugin> logger)
    {
        _logger = logger;
        _options = options.Value;
    }

    public void Configure(WebApplicationBuilder applicationBuilder, TracerProviderBuilder tracerBuilder)
    {
        if (_options.IsEnabled is false)
        {
            _logger.LogInformation("Sentry tracing is disabled");
            return;
        }

        tracerBuilder.AddSentry();
        _logger.LogInformation("Sentry tracing initialized");
    }
}