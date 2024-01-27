using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionResults;

public readonly record struct SuccessBuilder
{
    public ResultBuilder<TResult> WithResult<TResult>(TResult result) where TResult : IBackgroundTaskResult
        => new ResultBuilder<TResult>(result);

    public ResultBuilder<EmptyExecutionResult> WithEmptyResult()
        => new ResultBuilder<EmptyExecutionResult>();

    public readonly record struct ResultBuilder<TResult>(TResult Result) where TResult : IBackgroundTaskResult
    {
        public ErrorBuilder<TResult, TError> ForError<TError>() where TError : IBackgroundTaskError
            => new ErrorBuilder<TResult, TError>(Result);

        public ErrorBuilder<TResult, EmptyError> ForEmptyError()
            => new ErrorBuilder<TResult, EmptyError>();
    }

    public readonly record struct ErrorBuilder<TResult, TError>(TResult Result)
        where TResult : IBackgroundTaskResult
        where TError : IBackgroundTaskError
    {
        public static implicit operator BackgroundTaskExecutionResult<TResult, TError>(
            ErrorBuilder<TResult, TError> builder)
        {
            return new BackgroundTaskExecutionResult<TResult, TError>.Success(builder.Result);
        }
    }
}