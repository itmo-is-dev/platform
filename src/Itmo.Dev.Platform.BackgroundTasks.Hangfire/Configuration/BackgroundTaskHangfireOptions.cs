using Itmo.Dev.Platform.Options;
using System.ComponentModel;

namespace Itmo.Dev.Platform.BackgroundTasks.Hangfire.Configuration;

[OptionsType]
public class BackgroundTaskHangfireOptions
{
    [DefaultValue("00:00:10")]
    public TimeSpan CancellationCheckDelay { get; set; } = TimeSpan.FromSeconds(10);

    [DefaultValue(5)]
    public int SchedulerRetryCount { get; set; } = 5;

    [DefaultValue(new[] { 60, 60 * 2, 60 * 5, 60 * 10 })]
    public int[] SchedulerRetryDelays { get; set; } = [60, 60 * 2, 60 * 5, 60 * 10];

    [DefaultValue(1)]
    public int SchedulerWorkerCount { get; set; } = 1;
}
