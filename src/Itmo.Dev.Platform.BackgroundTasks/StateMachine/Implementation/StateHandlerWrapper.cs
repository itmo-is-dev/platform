using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.BackgroundTasks.StateMachine.Implementation;

internal class StateHandlerWrapper<TState, THandler, TStateBase, TMetadata, TExecutionMetadata, TResult, TError> :
    IStateHandlerWrapper<TStateBase, TMetadata, TExecutionMetadata, TResult, TError>
    where TState : TStateBase
    where THandler : class, IStateHandler<TState, TStateBase, TMetadata, TExecutionMetadata, TResult, TError>
    where TStateBase : IState
    where TMetadata : IBackgroundTaskMetadata
    where TExecutionMetadata : IStateExecutionMetadata<TStateBase>
    where TResult : IBackgroundTaskResult
    where TError : IBackgroundTaskError
{
    private readonly IServiceProvider _serviceProvider;

    public StateHandlerWrapper(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async ValueTask<StateHandleResult<TStateBase, TResult, TError>?> TryHandleAsync(
        TStateBase state,
        BackgroundTaskExecutionContext<TMetadata, TExecutionMetadata> context,
        CancellationToken cancellationToken)
    {
        if (state is not TState concreteState)
            return null;

        return await ActivatorUtilities
            .CreateInstance<THandler>(_serviceProvider)
            .HandleAsync(concreteState, context, cancellationToken);
    }
}