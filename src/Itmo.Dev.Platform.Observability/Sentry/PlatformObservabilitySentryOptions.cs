namespace Itmo.Dev.Platform.Observability.Sentry;

internal class PlatformObservabilitySentryOptions
{
    public bool IsEnabled { get; set; }
    
    public Uri? WebProxyUri { get; set; }
    
    public IConfigurationSection? Configuration { get; set; }
}