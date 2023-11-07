using Itmo.Dev.Platform.BackgroundTasks.Models;

namespace Itmo.Dev.Platform.BackgroundTasks.Execution;

public interface IBackgroundTaskManager
{
    Task ExecuteAsync(BackgroundTaskId id, CancellationToken cancellationToken);

    Task FailedAsync(BackgroundTaskId id, CancellationToken cancellationToken);
}