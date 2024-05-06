namespace Itmo.Dev.Platform.BackgroundTasks.Hangfire.Configuration;

public class BackgroundTaskHangfireOptions
{
    public TimeSpan CancellationCheckDelay { get; set; } = TimeSpan.FromSeconds(10);

    public int SchedulerRetryCount { get; set; } = 5;

    public int[] SchedulerRetryDelays { get; set; } = [60, 60 * 2, 60 * 5, 60 * 10];

    public int SchedulerWorkerCount { get; set; } = 1;
}