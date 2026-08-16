using Itmo.Dev.Platform.Options;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.Observability.HealthChecks;

[OptionsType]
internal class PlatformHealthCheckOptions
{
    [Required]
    public bool IsEnabled { get; set; }

    [DefaultValue("/health/startup")]
    public string StartupCheckUri { get; set; } = "/health/startup";

    [DefaultValue("/health/readyz")]
    public string ReadinessCheckUri { get; set; } = "/health/readyz";

    [DefaultValue("/health/livez")]
    public string LivenessCheckUri { get; set; } = "/health/livez";
}
