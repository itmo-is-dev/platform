using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;

namespace Itmo.Dev.Platform.BackgroundTasks.Tasks;

public record BackgroundTaskExecutionContext<TMetadata>(BackgroundTaskId Id, TMetadata Metadata)
    where TMetadata : IBackgroundTaskMetadata;