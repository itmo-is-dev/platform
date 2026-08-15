using Itmo.Dev.Platform.Options;

namespace Itmo.Dev.Platform.Observability.Logging;

[OptionsType]
internal class PlatformLoggingOptions
{
    public IConfigurationSection? Serilog { get; set; }
}