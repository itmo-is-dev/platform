namespace Itmo.Dev.Platform.Observability.Sentry;

internal class PlatformObservabilitySentryOptions
{
    public bool IsEnabled { get; set; }
    
    public IConfigurationSection? Configuration { get; set; }
}