using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionResults;

public readonly record struct SuccessBuilder
{
    public readonly record struct ResultBuilder
    {
        public ErrorBuilder<TResult> WithResult<TResult>(TResult result)
            where TResult : IBackgroundTaskResult
        {
            return new ErrorBuilder<TResult>(result);
        }

        public ErrorBuilder<EmptyExecutionResult> WithEmptyResult() => WithResult(EmptyExecutionResult.Value);
    }

    public readonly record struct ErrorBuilder<TResult>(TResult Result)
        where TResult : IBackgroundTaskResult
    {
        public CastHandle<TResult, TError> ForError<TError>()
            where TError : IBackgroundTaskError
        {
            return new CastHandle<TResult, TError>(Result);
        }

        public CastHandle<TResult, EmptyError> ForEmptyError() => ForError<EmptyError>();
    }

    public readonly record struct CastHandle<TResult, TError>(TResult Result)
        where TResult : IBackgroundTaskResult
        where TError : IBackgroundTaskError
    {
        public static implicit operator BackgroundTaskExecutionResult<TResult, TError>(
            CastHandle<TResult, TError> builder)
        {
            return new BackgroundTaskExecutionResult<TResult, TError>.Success(builder.Result);
        }
    }
}