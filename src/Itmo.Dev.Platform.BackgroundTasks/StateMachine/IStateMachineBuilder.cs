using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.StateMachine;

public interface IStateMachineBuilder<TStateBase, TMetadata, TExecutionMetadata, TResult, TError>
    where TStateBase : IState
    where TMetadata : IBackgroundTaskMetadata
    where TExecutionMetadata : IStateExecutionMetadata<TStateBase>
    where TResult : IBackgroundTaskResult
    where TError : IBackgroundTaskError
{
    IStateMachineBuilder<TStateBase, TMetadata, TExecutionMetadata, TResult, TError> WithState<TState, THandler>()
        where TState : TStateBase
        where THandler : class, IStateHandler<TState, TStateBase, TMetadata, TExecutionMetadata, TResult, TError>;

    IStateMachine<TStateBase, TMetadata, TExecutionMetadata, TResult, TError> Build();
}