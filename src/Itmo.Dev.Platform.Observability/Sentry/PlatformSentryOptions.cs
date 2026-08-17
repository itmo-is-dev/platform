using Itmo.Dev.Platform.Options;
using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.Observability.Sentry;

[OptionsType]
public class PlatformSentryOptions
{
    [Required]
    public bool IsEnabled { get; set; }

    public IConfigurationSection? Configuration { get; set; }
}
