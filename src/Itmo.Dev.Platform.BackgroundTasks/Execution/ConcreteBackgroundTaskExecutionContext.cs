using Itmo.Dev.Platform.BackgroundTasks.Configuration;
using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.Execution;

internal sealed class ConcreteBackgroundTaskExecutionContext<TMetadata, TExecutionMetadata>
    : BackgroundTaskExecutionContext<TMetadata, TExecutionMetadata>
    where TMetadata : IBackgroundTaskMetadata
    where TExecutionMetadata : IBackgroundTaskExecutionMetadata
{
    private readonly IBackgroundTaskInfrastructureRepository _backgroundTaskRepository;
    private readonly BackgroundTaskExecutionOptions _options;
    private BackgroundTask _backgroundTask;

    public ConcreteBackgroundTaskExecutionContext(
        BackgroundTask backgroundTask,
        IBackgroundTaskInfrastructureRepository backgroundTaskRepository,
        BackgroundTaskExecutionOptions options) : base(
        backgroundTask.Id,
        (TMetadata)backgroundTask.Metadata,
        (TExecutionMetadata)backgroundTask.ExecutionMetadata)
    {
        _backgroundTask = backgroundTask;
        _backgroundTaskRepository = backgroundTaskRepository;
        _options = options;
    }

    public override async Task PersistAsync(CancellationToken cancellationToken)
    {
        await _backgroundTaskRepository.UpdateAsync(_backgroundTask, cancellationToken);
    }

    internal void HandleResult<TResult, TError>(BackgroundTaskExecutionResult<TResult, TError> result)
        where TResult : IBackgroundTaskResult
        where TError : IBackgroundTaskError
    {
        _backgroundTask = result switch
        {
            BackgroundTaskExecutionResult<TResult, TError>.Success success
                => HandleSuccess(_backgroundTask, success),

            BackgroundTaskExecutionResult<TResult, TError>.Suspended suspended
                => HandleSuspended(_backgroundTask, suspended),

            BackgroundTaskExecutionResult<TResult, TError>.Failure failure
                => HandleFailure(_backgroundTask, failure),

            BackgroundTaskExecutionResult<TResult, TError>.Cancellation cancellation
                => HandleCancelled(_backgroundTask, cancellation),

            _ => HandleFailure(_backgroundTask, new BackgroundTaskExecutionResult<TResult, TError>.Failure(default)),
        };
    }

    private BackgroundTask HandleSuccess<TResult, TError>(
        BackgroundTask backgroundTask,
        BackgroundTaskExecutionResult<TResult, TError>.Success success)
        where TResult : IBackgroundTaskResult
        where TError : IBackgroundTaskError
    {
        return backgroundTask with
        {
            State = BackgroundTaskState.Completed,
            Result = success.Result,
        };
    }

    private BackgroundTask HandleSuspended<TResult, TError>(
        BackgroundTask backgroundTask,
        BackgroundTaskExecutionResult<TResult, TError>.Suspended suspended)
        where TResult : IBackgroundTaskResult
        where TError : IBackgroundTaskError
    {
        return suspended.Until is null
            ? backgroundTask with { State = BackgroundTaskState.Suspended }
            : backgroundTask with { State = BackgroundTaskState.Pending, ScheduledAt = suspended.Until.Value };
    }

    private BackgroundTask HandleFailure<TResult, TError>(
        BackgroundTask backgroundTask,
        BackgroundTaskExecutionResult<TResult, TError>.Failure failure)
        where TResult : IBackgroundTaskResult
        where TError : IBackgroundTaskError
    {
        return backgroundTask with
        {
            State = backgroundTask.RetryNumber < _options.MaxRetryCount
                ? BackgroundTaskState.Retrying
                : BackgroundTaskState.Failed,

            RetryNumber = backgroundTask.RetryNumber + 1,
            Error = failure.Error,
        };
    }

    private BackgroundTask HandleCancelled<TResult, TError>(
        BackgroundTask backgroundTask,
        BackgroundTaskExecutionResult<TResult, TError>.Cancellation cancellation)
        where TResult : IBackgroundTaskResult
        where TError : IBackgroundTaskError
    {
        return backgroundTask with
        {
            State = BackgroundTaskState.Cancelled,
            Error = cancellation.Error,
        };
    }
}
