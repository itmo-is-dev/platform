using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Itmo.Dev.Platform.Observability.HealthChecks.Startup;

public class ApplicationStartedCheck : IPlatformStartupHealthCheck, IHostedLifecycleService
{
    private static readonly Task<HealthCheckResult> NotStartedResult = Task.FromResult(HealthCheckResult.Unhealthy());
    private static readonly Task<HealthCheckResult> StartedResult = Task.FromResult(HealthCheckResult.Healthy());

    private Task<HealthCheckResult> _result = NotStartedResult;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
        => _result;

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        _result = StartedResult;
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
