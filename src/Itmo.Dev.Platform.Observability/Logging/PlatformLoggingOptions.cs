using Itmo.Dev.Platform.Options;

namespace Itmo.Dev.Platform.Observability.Logging;

[OptionsType]
public class PlatformLoggingOptions
{
    public IConfigurationSection? Serilog { get; set; }
}