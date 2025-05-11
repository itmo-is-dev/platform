using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Observability.HealthChecks.Plugins;

internal class HealthCheckApplicationPlugin : IObservabilityApplicationPlugin
{
    private readonly PlatformHealthCheckOptions _options;
    private readonly ILogger<HealthCheckApplicationPlugin> _logger;

    public HealthCheckApplicationPlugin(
        IOptions<PlatformHealthCheckOptions> options,
        ILogger<HealthCheckApplicationPlugin> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public void Configure(WebApplication application)
    {
        if (_options.IsEnabled is false)
        {
            _logger.LogInformation("HealthChecks is disabled");
            return;
        }

        application.MapHealthChecks(
            _options.StartupCheckUri,
            new HealthCheckOptions
            {
                Predicate = x => x.Factory.Method.ReturnType.IsAssignableTo(typeof(IPlatformStartupHealthCheck)),
            });

        application.MapHealthChecks(
            _options.ReadinessCheckUri,
            new HealthCheckOptions
            {
                Predicate = x => x.Factory.Method.ReturnType.IsAssignableTo(typeof(IPlatformReadinessHealthCheck)),
            });

        application.MapHealthChecks(
            _options.LivenessCheckUri,
            new HealthCheckOptions
            {
                Predicate = x => x.Factory.Method.ReturnType.IsAssignableTo(typeof(IPlatformLivenessHealthCheck)),
            });

        _logger.LogInformation("HealthChecks initialized");
    }
}