using Itmo.Dev.Platform.Observability.Logging;
using Serilog;
using Serilog.Events;

namespace Itmo.Dev.Platform.Observability.HealthChecks.Plugins;

internal class HealthChecksSerilogPlugin : ISerilogConfigurationPlugin
{
    public LoggerConfiguration Configure(WebApplicationBuilder builder, LoggerConfiguration loggerConfiguration)
    {
        return loggerConfiguration
            .MinimumLevel.Override("Microsoft.Extensions.Diagnostics.HealthChecks", LogEventLevel.Information);
    }
}