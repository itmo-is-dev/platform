using Itmo.Dev.Platform.BackgroundTasks.Models;

namespace Itmo.Dev.Platform.BackgroundTasks.Scheduling;

public interface IBackgroundTaskScheduler
{
    ValueTask ScheduleAsync(IEnumerable<BackgroundTaskId> ids, CancellationToken cancellationToken);
}