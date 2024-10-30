using Itmo.Dev.Platform.Common.Lifetime;
using Itmo.Dev.Platform.Observability.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Itmo.Dev.Platform.Observability.Platform.HealthChecks;

internal class PlatformLifetimeReadinessCheck : IPlatformStartupHealthCheck
{
    private readonly Task _lifetimeTask;

    public PlatformLifetimeReadinessCheck(IPlatformLifetime lifetime)
    {
        _lifetimeTask = lifetime.WaitOnInitializedAsync(default);
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        if (_lifetimeTask.IsCompletedSuccessfully)
        {
            return Task.FromResult(HealthCheckResult.Healthy());
        }

        if (_lifetimeTask.IsFaulted)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Failed to initialize lifetime",
                _lifetimeTask.Exception));
        }

        return Task.FromResult(HealthCheckResult.Unhealthy("Platform initialization in progress"));
    }
}