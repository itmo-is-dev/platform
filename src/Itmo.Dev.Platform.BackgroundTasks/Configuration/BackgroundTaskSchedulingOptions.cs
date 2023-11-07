namespace Itmo.Dev.Platform.BackgroundTasks.Configuration;

public class BackgroundTaskSchedulingOptions
{
    public int BatchSize { get; set; }

    public TimeSpan PollingDelay { get; set; }

    public int SchedulerRetryCount { get; set; } = 5;

    public int[] SchedulerRetryDelays { get; set; } = { 60, 60 * 2, 60 * 5, 60 * 10 };
}