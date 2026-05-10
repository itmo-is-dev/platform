using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;

namespace Itmo.Dev.Platform.BackgroundTasks.Tasks;

public abstract class BackgroundTaskExecutionContext<TMetadata, TExecutionMetadata>
    where TMetadata : IBackgroundTaskMetadata
    where TExecutionMetadata : IBackgroundTaskExecutionMetadata
{
    protected BackgroundTaskExecutionContext(
        BackgroundTaskId id,
        TMetadata metadata,
        TExecutionMetadata executionMetadata)
    {
        Id = id;
        Metadata = metadata;
        ExecutionMetadata = executionMetadata;
    }

    public BackgroundTaskId Id { get; }
    public TMetadata Metadata { get; }
    public TExecutionMetadata ExecutionMetadata { get; }

    public abstract Task PersistAsync(CancellationToken cancellationToken);
}
