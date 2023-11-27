using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;

namespace Itmo.Dev.Platform.BackgroundTasks.Managing.Proceeding;

internal class ExecutionMetadataModification
{
    private readonly Func<IBackgroundTaskExecutionMetadata, ExecutionMetadataModificationResult> _func;

    public ExecutionMetadataModification(
        Func<IBackgroundTaskExecutionMetadata, ExecutionMetadataModificationResult> func)
    {
        _func = func;
    }

    public ExecutionMetadataModificationResult Modify(IBackgroundTaskExecutionMetadata metadata)
    {
        return _func.Invoke(metadata);
    }
}