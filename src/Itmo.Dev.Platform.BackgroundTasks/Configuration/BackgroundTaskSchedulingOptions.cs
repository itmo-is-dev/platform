using Itmo.Dev.Platform.Options;

namespace Itmo.Dev.Platform.BackgroundTasks.Configuration;

[OptionsType]
public class BackgroundTaskSchedulingOptions
{
    public int BatchSize { get; set; }

    public TimeSpan PollingDelay { get; set; }
}