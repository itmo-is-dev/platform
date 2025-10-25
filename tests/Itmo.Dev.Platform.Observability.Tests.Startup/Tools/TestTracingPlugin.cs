using Itmo.Dev.Platform.Observability.Tracing;
using OpenTelemetry.Trace;

namespace Itmo.Dev.Platform.Observability.Tests.Startup.Tools;

public class TestTracingPlugin : ITracingConfigurationPlugin
{
    public void Configure(WebApplicationBuilder applicationBuilder, TracerProviderBuilder tracerBuilder)
    {
        tracerBuilder.AddProcessor(p => p.GetRequiredService<TestingActivityProcessor>());
    }
}
