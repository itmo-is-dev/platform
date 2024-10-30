using Itmo.Dev.Platform.Observability.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace Itmo.Dev.Platform.Observability.Sentry;

internal class SentrySerilogPlugin : ISerilogConfigurationPlugin
{
    private readonly PlatformSentryOptions _options;
    private readonly ILogger<SentrySerilogPlugin> _logger;

    public SentrySerilogPlugin(
        IOptions<PlatformSentryOptions> options,
        ILogger<SentrySerilogPlugin> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public LoggerConfiguration Configure(WebApplicationBuilder builder, LoggerConfiguration loggerConfiguration)
    {
        if (_options.IsEnabled is false)
        {
            _logger.LogInformation("Sentry logging is disabled");
            return loggerConfiguration;
        }

        loggerConfiguration = loggerConfiguration
            .WriteTo.Sentry(options => _options.Configuration?.Bind(options));

        _logger.LogInformation("Sentry logging initialized");

        return loggerConfiguration;
    }
}