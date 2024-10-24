namespace Itmo.Dev.Platform.Observability;

public interface IObservabilityApplicationPlugin
{
    void Configure(WebApplication application);
}