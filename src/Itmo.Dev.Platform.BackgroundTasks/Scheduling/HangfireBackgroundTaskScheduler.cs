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
            _backgroundJobClient.Enqueue<IBackgroundTaskExecutor>(
                e => e.ExecuteAsync(backgroundTaskId, default));
        }

        return ValueTask.CompletedTask;
    }
}