using OpenTelemetry.Trace;

namespace Itmo.Dev.Platform.Observability.Tracing;

internal interface ITracingConfigurationPlugin
{
    void Configure(WebApplicationBuilder applicationBuilder, TracerProviderBuilder tracerBuilder);
}