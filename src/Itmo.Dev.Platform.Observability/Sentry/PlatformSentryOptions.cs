namespace Itmo.Dev.Platform.Observability.Sentry;

internal class PlatformSentryOptions
{
    public bool IsEnabled { get; set; }
    
    public IConfigurationSection? Configuration { get; set; }
}