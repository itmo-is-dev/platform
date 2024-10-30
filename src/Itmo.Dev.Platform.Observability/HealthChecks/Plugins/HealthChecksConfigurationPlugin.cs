using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Observability.HealthChecks.Plugins;

internal class HealthChecksConfigurationPlugin : IObservabilityConfigurationPlugin
{
    private readonly PlatformHealthCheckOptions _options;
    private readonly IEnumerable<IHealthCheckConfigurationPlugin> _plugins;
    private readonly ILogger<HealthChecksConfigurationPlugin> _logger;

    public HealthChecksConfigurationPlugin(
        IOptions<PlatformHealthCheckOptions> options,
        IEnumerable<IHealthCheckConfigurationPlugin> plugins,
        ILogger<HealthChecksConfigurationPlugin> logger)
    {
        _options = options.Value;
        _plugins = plugins;
        _logger = logger;
    }

    public void Configure(WebApplicationBuilder builder)
    {
        if (_options.IsEnabled is false)
        {
            _logger.LogInformation("HealthChecks is disabled");
            return;
        }

        var healthChecksBuilder = builder.Services.AddHealthChecks();

        foreach (IHealthCheckConfigurationPlugin plugin in _plugins)
        {
            plugin.Configure(builder, healthChecksBuilder);
        }

        _logger.LogInformation("HealthChecks initialized");
    }
}