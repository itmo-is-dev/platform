using Serilog;

namespace Itmo.Dev.Platform.Observability.Logging;

internal interface ISerilogConfigurationPlugin
{
    LoggerConfiguration Configure(WebApplicationBuilder builder, LoggerConfiguration loggerConfiguration);
}