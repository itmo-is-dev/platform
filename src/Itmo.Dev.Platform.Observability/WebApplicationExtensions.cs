namespace Itmo.Dev.Platform.Observability;

public static class WebApplicationExtensions
{
    public static void UsePlatformObservability(this WebApplication application)
    {
        var plugins = application.Services.GetRequiredService<IEnumerable<IObservabilityApplicationPlugin>>();

        foreach (IObservabilityApplicationPlugin plugin in plugins)
        {
            plugin.Configure(application);
        }
    }
}