using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.Tasks;

public abstract record BackgroundTaskExecutionResult<TResult, TError>
    where TResult : IBackgroundTaskResult
    where TError : IBackgroundTaskError
{
    public sealed record Success(TResult Result) : BackgroundTaskExecutionResult<TResult, TError>;

    public sealed record Failure(TError? Error) : BackgroundTaskExecutionResult<TResult, TError>;

    public sealed record Cancellation(TError? Error) : BackgroundTaskExecutionResult<TResult, TError>;
}