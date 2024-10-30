using OpenTelemetry.Trace;

namespace Itmo.Dev.Platform.Observability.Tracing;

public interface ITracingConfigurationPlugin
{
    void Configure(WebApplicationBuilder applicationBuilder, TracerProviderBuilder tracerBuilder);
}