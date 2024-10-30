using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Observability.Metrics;

internal class MetricsApplicationPlugin : IObservabilityApplicationPlugin
{
    private readonly PlatformMetricsOptions _metricsOptions;
    private readonly ILogger<MetricsApplicationPlugin> _logger;

    public MetricsApplicationPlugin(
        IOptions<PlatformMetricsOptions> metricsOptions,
        ILogger<MetricsApplicationPlugin> logger)
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