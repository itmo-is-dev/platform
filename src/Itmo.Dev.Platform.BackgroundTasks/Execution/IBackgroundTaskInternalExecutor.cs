using Itmo.Dev.Platform.BackgroundTasks.Models;

namespace Itmo.Dev.Platform.BackgroundTasks.Execution;

internal interface IBackgroundTaskInternalExecutor
{
    Task ExecuteAsync(BackgroundTask backgroundTask, CancellationToken cancellationToken);
}