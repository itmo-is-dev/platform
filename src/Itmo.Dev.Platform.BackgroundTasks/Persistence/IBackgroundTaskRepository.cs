using Itmo.Dev.Platform.BackgroundTasks.Models;

namespace Itmo.Dev.Platform.BackgroundTasks.Persistence;

public interface IBackgroundTaskRepository
{
    IAsyncEnumerable<BackgroundTask> QueryAsync(BackgroundTaskQuery query, CancellationToken cancellationToken);
}