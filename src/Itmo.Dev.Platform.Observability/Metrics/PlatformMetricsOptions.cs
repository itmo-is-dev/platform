using Itmo.Dev.Platform.Options;
using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.Observability.Metrics;

[OptionsType]
internal class PlatformMetricsOptions
{
    [Required]
    public bool IsEnabled { get; set; }
}