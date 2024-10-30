using Serilog;

namespace Itmo.Dev.Platform.Observability.Logging;

public interface ISerilogConfigurationPlugin
{
    LoggerConfiguration Configure(WebApplicationBuilder builder, LoggerConfiguration loggerConfiguration);
}