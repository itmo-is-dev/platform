using Itmo.Dev.Platform.Options;

namespace Itmo.Dev.Platform.Observability.Sentry;

[OptionsType]
internal class PlatformSentryOptions
{
    public bool IsEnabled { get; set; }
    
    public IConfigurationSection? Configuration { get; set; }
}