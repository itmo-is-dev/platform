using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.StateMachine.Implementation;

internal class StateMachineBuilder<TStateBase, TMetadata, TExecutionMetadata, TResult, TError> : IStateMachineBuilder<
    TStateBase,
    TMetadata,
    TExecutionMetadata,
    TResult,
    TError>
    where TStateBase : IState
    where TMetadata : IBackgroundTaskMetadata
    where TExecutionMetadata : IStateExecutionMetadata<TStateBase>
    where TResult : IBackgroundTaskResult
    where TError : IBackgroundTaskError
{
    private readonly List<IStateHandlerWrapper<TStateBase, TMetadata, TExecutionMetadata, TResult, TError>> _wrappers;
    private readonly IServiceProvider _serviceProvider;

    public StateMachineBuilder(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _wrappers = new List<IStateHandlerWrapper<TStateBase, TMetadata, TExecutionMetadata, TResult, TError>>();
    }

    public IStateMachineBuilder<TStateBase, TMetadata, TExecutionMetadata, TResult, TError> WithState<
        TState,
        THandler>()
        where TState : TStateBase
        where THandler : class, IStateHandler<TState, TStateBase, TMetadata, TExecutionMetadata, TResult, TError>
    {
        var wrapper = new StateHandlerWrapper<
            TState,
            THandler,
            TStateBase,
            TMetadata,
            TExecutionMetadata,
            TResult,
            TError>(_serviceProvider);

        _wrappers.Add(wrapper);

        return this;
    }

    public IStateMachine<TStateBase, TMetadata, TExecutionMetadata, TResult, TError> Build()
    {
        return new StateMachine<TStateBase, TMetadata, TExecutionMetadata, TResult, TError>(_wrappers);
    }
}