using Itmo.Dev.Platform.Options;
using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.BackgroundTasks.Configuration;

[OptionsType]
public class BackgroundTaskSchedulingOptions
{
    [Required]
    [Range(minimum: 1, maximum: int.MaxValue)]
    public int BatchSize { get; set; }

    [Required]
    [Range(typeof(TimeSpan), minimum: "00:00:00.020", maximum: "1.00:00:00")]
    public TimeSpan PollingDelay { get; set; }
}
