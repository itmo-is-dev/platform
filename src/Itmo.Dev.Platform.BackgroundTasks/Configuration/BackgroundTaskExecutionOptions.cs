using Itmo.Dev.Platform.Options;

namespace Itmo.Dev.Platform.BackgroundTasks.Configuration;

[OptionsType]
public class BackgroundTaskExecutionOptions
{
    public int MaxRetryCount { get; set; }
}