using Itmo.Dev.Platform.Common.Options;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace Itmo.Dev.Platform.Observability.Metrics;

internal class MetricsObservabilityConfigurationPlugin : IObservabilityConfigurationPlugin
{
    private readonly PlatformOptions _platformOptions;
    private readonly PlatformObservabilityMetricsOptions _metricsOptions;
    private readonly ILogger<MetricsObservabilityConfigurationPlugin> _logger;

    public MetricsObservabilityConfigurationPlugin(
        IOptions<PlatformOptions> platformOptions,
        IOptions<PlatformObservabilityMetricsOptions> metricsOptions,
        ILogger<MetricsObservabilityConfigurationPlugin> logger)
    {
        _logger = logger;
        _platformOptions = platformOptions.Value;
        _metricsOptions = metricsOptions.Value;
    }

    public void Configure(WebApplicationBuilder builder)
    {
        if (_metricsOptions.IsEnabled is false)
        {
            _logger.LogInformation("OpenTelemetry metrics is disabled");
            return;
        }

        builder.Services
            .AddOpenTelemetry()
            .WithMetrics(metrics => metrics
                .ConfigureResource(x => x.AddService(_platformOptions.ServiceName))
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddPrometheusExporter());

        builder.Services.AddMetrics();

        _logger.LogInformation("OpenTelemetry metrics initialized");
    }
}