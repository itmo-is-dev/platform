using Itmo.Dev.Platform.Options;
using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.Observability.Tracing;

[OptionsType]
public class PlatformTracingOptions
{
    [Required]
    public bool IsEnabled { get; set; }

    public string[] Sources { get; set; } = [];
}