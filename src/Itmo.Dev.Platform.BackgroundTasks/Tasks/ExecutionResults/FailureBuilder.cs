using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionResults;

public readonly record struct FailureBuilder
{
    public readonly record struct ResultBuilder
    {
        public ErrorBuilder<TResult> ForResult<TResult>()
            where TResult : IBackgroundTaskResult
        {
            return new ErrorBuilder<TResult>();
        }

        public ErrorBuilder<EmptyExecutionResult> ForEmptyResult() => ForResult<EmptyExecutionResult>();
    }

    public readonly record struct ErrorBuilder<TResult>
        where TResult : IBackgroundTaskResult
    {
        public CastHandle<TResult, TError> WithError<TError>(TError? error = default)
            where TError : IBackgroundTaskError
        {
            return new CastHandle<TResult, TError>(error);
        }

        public CastHandle<TResult, EmptyError> WithEmptyError() => WithError(EmptyError.Value);
    }

    public readonly record struct CastHandle<TResult, TError>(TError? Error)
        where TResult : IBackgroundTaskResult
        where TError : IBackgroundTaskError
    {
        public static implicit operator BackgroundTaskExecutionResult<TResult, TError>(
            CastHandle<TResult, TError> builder)
        {
            return new BackgroundTaskExecutionResult<TResult, TError>.Failure(builder.Error);
        }
    }
}