using Itmo.Dev.Platform.BackgroundTasks.Models;

namespace Itmo.Dev.Platform.BackgroundTasks.Extensions;

public static class BackgroundTaskQueryExtensions
{
    public static BackgroundTaskQuery.Builder WithActiveState(this BackgroundTaskQuery.Builder builder)
    {
        return builder
            .WithState(BackgroundTaskState.Pending)
            .WithState(BackgroundTaskState.Enqueued)
            .WithState(BackgroundTaskState.Proceeded)
            .WithState(BackgroundTaskState.Retrying)
            .WithState(BackgroundTaskState.Suspended);
    }
}