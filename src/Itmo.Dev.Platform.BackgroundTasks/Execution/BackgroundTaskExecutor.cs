using Itmo.Dev.Platform.BackgroundTasks.Configuration;
using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.BackgroundTasks.Execution;

internal class BackgroundTaskExecutor<TTask, TMetadata, TExecutionMetadata, TResult, TError> :
    IBackgroundTaskInternalExecutor
    where TMetadata : IBackgroundTaskMetadata
    where TExecutionMetadata : IBackgroundTaskExecutionMetadata
    where TResult : IBackgroundTaskResult
    where TError : IBackgroundTaskError
    where TTask : IBackgroundTask<TMetadata, TExecutionMetadata, TResult, TError>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly BackgroundTaskExecutionOptions _options;
    private readonly IBackgroundTaskInfrastructureRepository _repository;
    private readonly ILogger<BackgroundTaskExecutor<TTask, TMetadata, TExecutionMetadata, TResult, TError>> _logger;

    public BackgroundTaskExecutor(
        IServiceProvider serviceProvider,
        IOptions<BackgroundTaskExecutionOptions> options,
        IBackgroundTaskInfrastructureRepository repository,
        ILogger<BackgroundTaskExecutor<TTask, TMetadata, TExecutionMetadata, TResult, TError>> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _repository = repository;
        _logger = logger;
    }

    public async Task ExecuteAsync(
        BackgroundTask backgroundTask,
        CancellationToken cancellationToken)
    {
        var metadata = (TMetadata)backgroundTask.Metadata;
        var executionMetadata = (TExecutionMetadata)backgroundTask.ExecutionMetadata;

        var context = new BackgroundTaskExecutionContext<TMetadata, TExecutionMetadata>(
            backgroundTask.Id,
            metadata,
            executionMetadata);

        var task = _serviceProvider.GetRequiredService<TTask>();

        try
        {
            var result = await task.ExecuteAsync(context, cancellationToken);

            backgroundTask = result switch
            {
                BackgroundTaskExecutionResult<TResult, TError>.Success success
                    => HandleSuccess(backgroundTask, success),
                
                BackgroundTaskExecutionResult<TResult, TError>.Suspended suspended
                    => HandleSuspended(backgroundTask, suspended),

                BackgroundTaskExecutionResult<TResult, TError>.Failure failure
                    => HandleFailure(backgroundTask, failure),

                BackgroundTaskExecutionResult<TResult, TError>.Cancellation cancellation
                    => HandleCancelled(backgroundTask, cancellation),

                _ => HandleFailure(backgroundTask, new BackgroundTaskExecutionResult<TResult, TError>.Failure(default)),
            };

            await _repository.UpdateAsync(backgroundTask, cancellationToken);
        }
        catch (Exception e) when (e is OperationCanceledException or TaskCanceledException)
        {
            await _repository.UpdateAsync(backgroundTask, default);
        }
    }

    private BackgroundTask HandleSuccess(
        BackgroundTask backgroundTask,
        BackgroundTaskExecutionResult<TResult, TError>.Success success)
    {
        return backgroundTask with
        {
            State = BackgroundTaskState.Completed,
            Result = success.Result,
        };
    }

    private BackgroundTask HandleSuspended(
        BackgroundTask backgroundTask,
        BackgroundTaskExecutionResult<TResult, TError>.Suspended suspended)
    {
        return backgroundTask with
        {
            State = BackgroundTaskState.Suspended,
        };
    }

    private BackgroundTask HandleFailure(
        BackgroundTask backgroundTask,
        BackgroundTaskExecutionResult<TResult, TError>.Failure failure)
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

    private BackgroundTask HandleCancelled(
        BackgroundTask backgroundTask,
        BackgroundTaskExecutionResult<TResult, TError>.Cancellation cancellation)
    {
        return backgroundTask with
        {
            State = BackgroundTaskState.Cancelled,
            Error = cancellation.Error,
        };
    }
}