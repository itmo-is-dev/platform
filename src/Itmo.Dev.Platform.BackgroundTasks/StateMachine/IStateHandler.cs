using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.StateMachine;

public interface IStateHandler<in TState, TStateBase, TMetadata, TExecutionMetadata, TResult, TError>
    where TState : TStateBase
    where TStateBase : IState
    where TMetadata : IBackgroundTaskMetadata
    where TExecutionMetadata : IStateExecutionMetadata<TStateBase>
    where TResult : IBackgroundTaskResult
    where TError : IBackgroundTaskError
{
    ValueTask<StateHandleResult<TStateBase, TResult, TError>> HandleAsync(
        TState state,
        BackgroundTaskExecutionContext<TMetadata, TExecutionMetadata> context,
        CancellationToken cancellationToken);
}