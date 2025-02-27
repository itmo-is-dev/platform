using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionResults;

public readonly record struct SuspendedBuilder
{
    public readonly record struct ResultBuilder
    {
        public ErrorBuilder<TResult> ForResult<TResult>()
            where TResult : IBackgroundTaskResult
            => new ErrorBuilder<TResult>();

        public ErrorBuilder<EmptyExecutionResult> ForEmptyResult()
            => new ErrorBuilder<EmptyExecutionResult>();
    }

    public readonly record struct ErrorBuilder<TResult>
        where TResult : IBackgroundTaskResult
    {
        public CastHandle<TResult, TError> ForError<TError>()
            where TError : IBackgroundTaskError
            => new CastHandle<TResult, TError>();

        public CastHandle<TResult, EmptyError> ForEmptyError()
            => new CastHandle<TResult, EmptyError>();
    }

    public readonly record struct CastHandle<TResult, TError>
        where TResult : IBackgroundTaskResult
        where TError : IBackgroundTaskError
    {
        private DateTimeOffset? UntilValue { get; init; }

        public CastHandle<TResult, TError> Until(DateTimeOffset value) => this with { UntilValue = value };

        public static implicit operator BackgroundTaskExecutionResult<TResult, TError>(
            CastHandle<TResult, TError> builder)
        {
            return new BackgroundTaskExecutionResult<TResult, TError>.Suspended(Until: builder.UntilValue);
        }
    }
}
