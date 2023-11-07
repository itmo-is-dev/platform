using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;

namespace Itmo.Dev.Platform.BackgroundTasks.Tasks;

public record BackgroundTaskExecutionContext<TMetadata, TExecutionMetadata>(
    BackgroundTaskId Id,
    TMetadata Metadata,
    TExecutionMetadata ExecutionMetadata)
    where TMetadata : IBackgroundTaskMetadata
    where TExecutionMetadata : IBackgroundTaskExecutionMetadata;