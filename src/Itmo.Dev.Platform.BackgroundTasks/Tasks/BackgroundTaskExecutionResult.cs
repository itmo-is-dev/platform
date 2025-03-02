using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionResults;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.Tasks;

public abstract record BackgroundTaskExecutionResult<TResult, TError>
    where TResult : IBackgroundTaskResult
    where TError : IBackgroundTaskError
{
    private BackgroundTaskExecutionResult() { }

    public sealed record Success : BackgroundTaskExecutionResult<TResult, TError>
    {
        internal Success(TResult result)
        {
            Result = result;
        }

        public TResult Result { get; internal init; }
    }

    public sealed record Suspended : BackgroundTaskExecutionResult<TResult, TError>
    {
        internal Suspended(DateTimeOffset? until)
        {
            Until = until;
        }

        public DateTimeOffset? Until { get; internal init; }
    }

    public sealed record Failure : BackgroundTaskExecutionResult<TResult, TError>
    {
        internal Failure(TError? error)
        {
            Error = error;
        }

        public TError? Error { get; internal init; }
    }

    public sealed record Cancellation : BackgroundTaskExecutionResult<TResult, TError>
    {
        internal Cancellation(TError? error)
        {
            Error = error;
        }

        public TError? Error { get; internal init; }
    }
}

public static class BackgroundTaskExecutionResult
{
    public static SuccessBuilder.ResultBuilder Success => new();

    public static SuspendedBuilder.ResultBuilder Suspended => new();

    public static FailureBuilder.ResultBuilder Failure => new();

    public static CancellationBuilder.ResultBuilder Cancellation => new();
}
