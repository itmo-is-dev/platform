using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.Tasks;

public interface IBackgroundTask<TMetadata, TExecutionMetadata>
    where TMetadata : IBackgroundTaskMetadata
    where TExecutionMetadata : IBackgroundTaskExecutionMetadata
{
    static abstract string Name { get; }
}

public interface IBackgroundTask<TMetadata, TExecutionMetadata, TResult, TError> :
    IBackgroundTask<TMetadata, TExecutionMetadata>
    where TMetadata : IBackgroundTaskMetadata
    where TExecutionMetadata : IBackgroundTaskExecutionMetadata
    where TResult : IBackgroundTaskResult
    where TError : IBackgroundTaskError
{
    Task<BackgroundTaskExecutionResult<TResult, TError>> ExecuteAsync(
        BackgroundTaskExecutionContext<TMetadata, TExecutionMetadata> executionContext,
        CancellationToken cancellationToken);
}