using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.Tasks;

public interface IBackgroundTask<TMetadata>
    where TMetadata : IBackgroundTaskMetadata
{
    static abstract string Name { get; }
}

public interface IBackgroundTask<TMetadata, TResult, TError> : IBackgroundTask<TMetadata>
    where TMetadata : IBackgroundTaskMetadata
    where TResult : IBackgroundTaskResult
    where TError : IBackgroundTaskError
{
    Task<BackgroundTaskExecutionResult<TResult, TError>> ExecuteAsync(
        BackgroundTaskExecutionContext<TMetadata> executionContext,
        CancellationToken cancellationToken);
}