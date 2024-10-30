namespace Itmo.Dev.Platform.Observability.HealthChecks;

public interface IHealthCheckConfigurationPlugin
{
    void Configure(WebApplicationBuilder builder, IHealthChecksBuilder healthChecksBuilder);
}