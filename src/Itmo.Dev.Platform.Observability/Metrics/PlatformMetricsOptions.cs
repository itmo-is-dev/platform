using Itmo.Dev.Platform.Options;
using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.Observability.Metrics;

[OptionsType]
public class PlatformMetricsOptions
{
    [Required]
    public bool IsEnabled { get; set; }
}