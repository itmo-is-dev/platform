using Hangfire;
using Itmo.Dev.Platform.BackgroundTasks.Execution;
using Itmo.Dev.Platform.BackgroundTasks.Models;

namespace Itmo.Dev.Platform.BackgroundTasks.Scheduling;

internal class HangfireBackgroundTaskScheduler : IBackgroundTaskScheduler
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public HangfireBackgroundTaskScheduler(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public ValueTask ScheduleAsync(IEnumerable<BackgroundTaskId> ids, CancellationToken cancellationToken)
    {
        foreach (BackgroundTaskId backgroundTaskId in ids)
        {
            var jobId = _backgroundJobClient.Enqueue<IBackgroundTaskManager>(
                x => x.ExecuteAsync(backgroundTaskId, CancellationToken.None));

            _backgroundJobClient.ContinueJobWith<IBackgroundTaskManager>(
                jobId,
                x => x.FailedAsync(backgroundTaskId, CancellationToken.None),
                JobContinuationOptions.OnlyOnDeletedState);
        }

        return ValueTask.CompletedTask;
    }
}