using Itmo.Dev.Platform.Options;

namespace Itmo.Dev.Platform.Observability.Metrics;

[OptionsType]
internal class PlatformMetricsOptions
{
    public bool IsEnabled { get; set; }
}