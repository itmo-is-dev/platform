using Itmo.Dev.Platform.Options;

namespace Itmo.Dev.Platform.Observability.Tracing;

[OptionsType]
internal class PlatformTracingOptions
{
    public bool IsEnabled { get; set; }

    public string[]? Sources { get; set; }
}