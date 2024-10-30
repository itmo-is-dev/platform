using Itmo.Dev.Platform.Observability.HealthChecks;
using Itmo.Dev.Platform.Observability.Platform.HealthChecks;

namespace Itmo.Dev.Platform.Observability.Platform;

internal class PlatformHealthCheckPlugin : IHealthCheckConfigurationPlugin
{
    public void Configure(WebApplicationBuilder builder, IHealthChecksBuilder healthChecksBuilder)
    {
        healthChecksBuilder.AddCheck<PlatformLifetimeReadinessCheck>(nameof(PlatformLifetimeReadinessCheck));
    }
}