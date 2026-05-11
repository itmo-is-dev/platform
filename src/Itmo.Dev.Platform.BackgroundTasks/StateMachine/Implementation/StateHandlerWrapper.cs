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
    private IStateHandlerWrapper<TStateBase, TMetadata, TExecutionMetadata, TResult, TError>? _next;

    public StateHandlerWrapper(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async ValueTask<StateHandleResult<TStateBase, TResult, TError>> HandleAsync(
        TStateBase state,
        BackgroundTaskExecutionContext<TMetadata, TExecutionMetadata> context,
        CancellationToken cancellationToken)
    {
        if (state is TState concreteState)
        {
            return await ActivatorUtilities
                .CreateInstance<THandler>(_serviceProvider)
                .HandleAsync(concreteState, context, cancellationToken);
        }

        if (_next is null)
        {
            throw new InvalidOperationException(
                $"Could not find handler for state = {context.ExecutionMetadata.State}");
        }

        return await _next.HandleAsync(state, context, cancellationToken);
    }

    public void SetNext(IStateHandlerWrapper<TStateBase, TMetadata, TExecutionMetadata, TResult, TError> wrapper)
    {
        if (_next is null)
        {
            _next = wrapper;
        }
        else
        {
            _next.SetNext(wrapper);
        }
    }
}
