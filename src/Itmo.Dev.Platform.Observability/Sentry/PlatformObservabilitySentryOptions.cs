namespace Itmo.Dev.Platform.Observability.Sentry;

internal class PlatformObservabilitySentryOptions
{
    public bool IsEnabled { get; set; }
    
    public Uri? WebProxyUri { get; set; }

    public string WebProxyUsername { get; set; } = string.Empty;

    public string WebProxyPassword { get; set; } = string.Empty;
    
    public IConfigurationSection? Configuration { get; set; }
}