using Itmo.Dev.Platform.Observability.HealthChecks;
using Itmo.Dev.Platform.Observability.Sentry.HealthChecks;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Observability.Sentry;

internal class SentryHealthChecksPlugin : IHealthCheckConfigurationPlugin
{
    private readonly PlatformSentryOptions _options;
    private readonly ILogger<SentryHealthChecksPlugin> _logger;

    public SentryHealthChecksPlugin(IOptions<PlatformSentryOptions> options, ILogger<SentryHealthChecksPlugin> logger)
    {
        _logger = logger;
        _options = options.Value;
    }

    public void Configure(WebApplicationBuilder builder, IHealthChecksBuilder healthChecksBuilder)
    {
        if (_options.IsEnabled is false)
        {
            _logger.LogInformation("Sentry HealthChecks are disabled");
            return;
        }

        healthChecksBuilder.AddCheck<SentrySdkHealthCheck>(nameof(SentrySdkHealthCheck));
        _logger.LogInformation("Sentry HealthChecks are initialized");
    }
}