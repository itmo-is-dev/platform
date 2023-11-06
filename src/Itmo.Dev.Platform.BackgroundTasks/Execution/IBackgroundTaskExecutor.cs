using Itmo.Dev.Platform.BackgroundTasks.Models;

namespace Itmo.Dev.Platform.BackgroundTasks.Execution;

public interface IBackgroundTaskExecutor
{
    Task ExecuteAsync(BackgroundTaskId id, CancellationToken cancellationToken);
}