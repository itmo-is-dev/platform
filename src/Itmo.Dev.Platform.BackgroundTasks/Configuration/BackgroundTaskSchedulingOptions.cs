namespace Itmo.Dev.Platform.BackgroundTasks.Configuration;

public class BackgroundTaskSchedulingOptions
{
    public int BatchSize { get; set; }

    public TimeSpan PollingDelay { get; set; }
}