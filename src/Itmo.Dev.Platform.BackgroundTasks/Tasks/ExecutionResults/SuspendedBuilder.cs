using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionResults;

public readonly record struct SuspendedBuilder
{
    public ResultBuilder<TResult> ForResult<TResult>() where TResult : IBackgroundTaskResult
        => new ResultBuilder<TResult>();

    public ResultBuilder<EmptyExecutionResult> ForEmptyResult()
        => new ResultBuilder<EmptyExecutionResult>();

    public readonly record struct ResultBuilder<TResult> where TResult : IBackgroundTaskResult
    {
        public ErrorBuilder<TResult, TError> ForError<TError>() where TError : IBackgroundTaskError
            => new ErrorBuilder<TResult, TError>();

        public ErrorBuilder<TResult, EmptyError> ForEmptyError()
            => new ErrorBuilder<TResult, EmptyError>();
    }

    public readonly record struct ErrorBuilder<TResult, TError>
        where TResult : IBackgroundTaskResult
        where TError : IBackgroundTaskError
    {
        public static implicit operator BackgroundTaskExecutionResult<TResult, TError>(
            ErrorBuilder<TResult, TError> builder)
        {
            return new BackgroundTaskExecutionResult<TResult, TError>.Suspended();
        }
    }
}