using Itmo.Dev.Platform.BackgroundTasks.Models;

namespace Itmo.Dev.Platform.BackgroundTasks.Persistence;

internal interface IBackgroundTaskInfrastructureRepository : IBackgroundTaskRepository
{
    IAsyncEnumerable<BackgroundTaskId> SearchIdsAsync(BackgroundTaskQuery query, CancellationToken cancellationToken);

    IAsyncEnumerable<BackgroundTaskId> AddRangeAsync(
        IReadOnlyCollection<BackgroundTask> tasks,
        CancellationToken cancellationToken);

    Task UpdateStateAsync(
        IEnumerable<BackgroundTaskId> ids,
        BackgroundTaskState state,
        CancellationToken cancellationToken);

    Task UpdateAsync(BackgroundTask backgroundTask, CancellationToken cancellationToken);
}