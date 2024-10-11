namespace Itmo.Dev.Platform.Observability;

internal interface IObservabilityConfigurationPlugin
{
    void Configure(WebApplicationBuilder builder);
}