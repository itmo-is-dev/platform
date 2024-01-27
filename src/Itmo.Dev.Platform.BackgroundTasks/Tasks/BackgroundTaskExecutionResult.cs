using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionResults;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.Tasks;

public abstract record BackgroundTaskExecutionResult<TResult, TError>
    where TResult : IBackgroundTaskResult
    where TError : IBackgroundTaskError
{
    private BackgroundTaskExecutionResult() { }

    internal sealed record Success(TResult Result) : BackgroundTaskExecutionResult<TResult, TError>;

    internal sealed record Suspended : BackgroundTaskExecutionResult<TResult, TError>;

    internal sealed record Failure(TError? Error) : BackgroundTaskExecutionResult<TResult, TError>;

    internal sealed record Cancellation(TError? Error) : BackgroundTaskExecutionResult<TResult, TError>;
}

public static class BackgroundTaskExecutionResult
{
    public static SuccessBuilder Success => new SuccessBuilder();

    public static SuspendedBuilder Suspended => new SuspendedBuilder();

    public static FailureBuilder Failure => new FailureBuilder();

    public static CancellationBuilder Cancellation => new CancellationBuilder();
}