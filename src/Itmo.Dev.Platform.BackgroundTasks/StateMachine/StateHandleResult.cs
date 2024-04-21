using Itmo.Dev.Platform.BackgroundTasks.StateMachine.StateHandleResults;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.StateMachine;

public abstract record StateHandleResult<TStateBase, TResult, TError>
    where TStateBase : IState where TResult : IBackgroundTaskResult where TError : IBackgroundTaskError
{
    private StateHandleResult() { }

    public sealed record Finished(TStateBase State) : StateHandleResult<TStateBase, TResult, TError>;

    public sealed record FinishedWithResult(
        TStateBase State,
        BackgroundTaskExecutionResult<TResult, TError> Result) : StateHandleResult<TStateBase, TResult, TError>;
}

public static class StateHandleResult
{
    public static FinishedBuilder.StateBaseBuilder Finished => new();
    public static FinishedWithResultBuilder.StateBaseBuilder FinishedWithResult => new();
}