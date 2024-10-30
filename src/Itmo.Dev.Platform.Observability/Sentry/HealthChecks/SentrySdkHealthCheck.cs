using Itmo.Dev.Platform.Observability.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Itmo.Dev.Platform.Observability.Sentry.HealthChecks;

internal class SentrySdkHealthCheck : IPlatformStartupHealthCheck, IPlatformReadinessHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        return SentrySdk.IsEnabled
            ? Task.FromResult(HealthCheckResult.Healthy())
            : Task.FromResult(new HealthCheckResult(HealthStatus.Unhealthy, "SentrySdk is not initialized"));
    }
}