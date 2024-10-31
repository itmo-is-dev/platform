namespace Itmo.Dev.Platform.Observability.HealthChecks;

internal class PlatformHealthCheckOptions
{
    public bool IsEnabled { get; set; }

    public string StartupCheckUri { get; set; } = "/health/startup";

    public string ReadinessCheckUri { get; set; } = "/health/readyz";

    public string LivenessCheckUri { get; set; } = "/health/livez";
}