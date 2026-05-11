using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.StateMachine.Implementation;

internal class StateMachine<TStateBase, TMetadata, TExecutionMetadata, TResult, TError> : IStateMachine<
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
    private readonly IStateHandlerWrapper<TStateBase, TMetadata, TExecutionMetadata, TResult, TError> _wrapper;

    public StateMachine(IStateHandlerWrapper<TStateBase, TMetadata, TExecutionMetadata, TResult, TError> wrapper)
    {
        _wrapper = wrapper;
    }

    public async ValueTask<BackgroundTaskExecutionResult<TResult, TError>> RunAsync(
        BackgroundTaskExecutionContext<TMetadata, TExecutionMetadata> context,
        CancellationToken cancellationToken)
    {
        var visitedStates = new HashSet<IState>();

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (visitedStates.Add(context.ExecutionMetadata.State) is false)
            {
                throw new InvalidOperationException(
                    $"Detected cycle, state = '{context.ExecutionMetadata.State}' visited multiple times in a single execution run");
            }

            var result = await _wrapper.HandleAsync(
                context.ExecutionMetadata.State,
                context,
                cancellationToken);

            switch (result)
            {
                case StateHandleResult<TStateBase, TResult, TError>.Finished finished:
                {
                    context.ExecutionMetadata.UpdateState(finished.State);
                    await context.PersistAsync(cancellationToken);

                    break;
                }
                case StateHandleResult<TStateBase, TResult, TError>.FinishedWithResult finishedWithResult:
                {
                    context.ExecutionMetadata.UpdateState(finishedWithResult.State);
                    return finishedWithResult.Result;
                }
                default:
                {
                    throw new InvalidOperationException($"Invalid state handler result = {result}");
                }
            }
        }
    }
}
