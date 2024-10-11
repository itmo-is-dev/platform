using Itmo.Dev.Platform.Observability.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace Itmo.Dev.Platform.Observability.Sentry;

internal class SentrySerilogPlugin : ISerilogConfigurationPlugin
{
    private readonly PlatformObservabilitySentryOptions _options;
    private readonly ILogger<SentrySerilogPlugin> _logger;

    public SentrySerilogPlugin(
        IOptions<PlatformObservabilitySentryOptions> options,
        ILogger<SentrySerilogPlugin> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public void Configure(WebApplicationBuilder builder, LoggerConfiguration loggerConfiguration)
    {
        if (_options.IsEnabled is false)
        {
            _logger.LogInformation("Sentry logging is disabled");
            return;
        }

        loggerConfiguration.WriteTo.Sentry(sentry => sentry.InitializeSdk = false);
        _logger.LogInformation("Sentry logging initialized");
    }
}