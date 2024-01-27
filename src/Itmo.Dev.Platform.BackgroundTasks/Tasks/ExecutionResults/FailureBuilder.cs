using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionResults;

public readonly record struct FailureBuilder
{
    public ResultBuilder<TResult> ForResult<TResult>() where TResult : IBackgroundTaskResult
        => new ResultBuilder<TResult>();

    public ResultBuilder<EmptyExecutionResult> ForEmptyResult()
        => new ResultBuilder<EmptyExecutionResult>();

    public readonly record struct ResultBuilder<TResult> where TResult : IBackgroundTaskResult
    {
        public ErrorBuilder<TResult, TError> WithError<TError>(TError? error = default)
            where TError : IBackgroundTaskError
        {
            return new ErrorBuilder<TResult, TError>(error);
        }

        public ErrorBuilder<TResult, EmptyError> WithEmptyError()
            => new ErrorBuilder<TResult, EmptyError>(EmptyError.Value);
    }

    public readonly record struct ErrorBuilder<TResult, TError>(TError? Error)
        where TResult : IBackgroundTaskResult
        where TError : IBackgroundTaskError
    {
        public static implicit operator BackgroundTaskExecutionResult<TResult, TError>(
            ErrorBuilder<TResult, TError> builder)
        {
            return new BackgroundTaskExecutionResult<TResult, TError>.Failure(builder.Error);
        }
    }
}