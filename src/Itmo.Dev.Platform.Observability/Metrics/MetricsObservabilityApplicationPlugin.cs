using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Observability.Metrics;

internal class MetricsObservabilityApplicationPlugin : IObservabilityApplicationPlugin
{
    private readonly PlatformObservabilityMetricsOptions _metricsOptions;
    private readonly ILogger<MetricsObservabilityApplicationPlugin> _logger;

    public MetricsObservabilityApplicationPlugin(
        IOptions<PlatformObservabilityMetricsOptions> metricsOptions,
        ILogger<MetricsObservabilityApplicationPlugin> logger)
    {
        _logger = logger;
        _metricsOptions = metricsOptions.Value;
    }

    public void Configure(WebApplication application)
    {
        if (_metricsOptions.IsEnabled is false)
        {
            _logger.LogInformation("OpenTelemetry metrics is disabled");
            return;
        }

        application.UseOpenTelemetryPrometheusScrapingEndpoint();
        _logger.LogInformation("OpenTelemetry Prometheus endpoint is initialized");
    }
}