using Itmo.Dev.Platform.Options;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.BackgroundTasks.Configuration;

[OptionsType]
public class BackgroundTaskExecutionOptions
{
    [DefaultValue(0)]
    [Range(minimum: 0, maximum: int.MaxValue)]
    public int MaxRetryCount { get; set; }
}