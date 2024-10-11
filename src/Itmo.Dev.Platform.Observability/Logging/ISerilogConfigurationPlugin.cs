using Serilog;

namespace Itmo.Dev.Platform.Observability.Logging;

internal interface ISerilogConfigurationPlugin
{
    void Configure(WebApplicationBuilder builder, LoggerConfiguration loggerConfiguration);
}