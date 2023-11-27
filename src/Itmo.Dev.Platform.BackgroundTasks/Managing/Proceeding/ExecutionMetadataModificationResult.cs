using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;

namespace Itmo.Dev.Platform.BackgroundTasks.Managing.Proceeding;

internal abstract record ExecutionMetadataModificationResult
{
    private ExecutionMetadataModificationResult() { }

    public sealed record Success(IBackgroundTaskExecutionMetadata Metadata) : ExecutionMetadataModificationResult;

    public sealed record Failure(string Message) : ExecutionMetadataModificationResult;
}