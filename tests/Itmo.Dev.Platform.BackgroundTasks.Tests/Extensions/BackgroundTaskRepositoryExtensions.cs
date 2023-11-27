using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Extensions;

public static class BackgroundTaskRepositoryExtensions
{
    public static async Task<BackgroundTask> GetBackgroundTaskAsync(
        this IBackgroundTaskRepository repository, 
        BackgroundTaskId id)
    {
        var query = BackgroundTaskQuery.Build(x => x.WithId(id));
        return await repository.QueryAsync(query, default).SingleAsync();
    }
}