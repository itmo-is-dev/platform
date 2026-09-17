using Itmo.Dev.Platform.Options;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.Observability.Metrics;

[OptionsType]
public class PlatformMetricsOptions
{
    [Required]
    public bool IsEnabled { get; set; }

    [Description("Names of Meters that should emit metrics")]
    public string[] MeterNames { get; set; } = [];
}