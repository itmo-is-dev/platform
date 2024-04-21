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
    private readonly
        IReadOnlyCollection<IStateHandlerWrapper<TStateBase, TMetadata, TExecutionMetadata, TResult, TError>> _wrappers;

    public StateMachine(
        IReadOnlyCollection<IStateHandlerWrapper<TStateBase, TMetadata, TExecutionMetadata, TResult, TError>> wrappers)
    {
        _wrappers = wrappers;
    }

    public async ValueTask<BackgroundTaskExecutionResult<TResult, TError>> RunAsync(
        BackgroundTaskExecutionContext<TMetadata, TExecutionMetadata> context,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var handled = false;

            foreach (var wrapper in _wrappers)
            {
                var result = await wrapper.TryHandleAsync(
                    context.ExecutionMetadata.State,
                    context,
                    cancellationToken);

                if (result is null)
                    continue;

                if (result is StateHandleResult<TStateBase, TResult, TError>.Finished finished)
                {
                    handled = true;
                    context.ExecutionMetadata.UpdateState(finished.State);

                    break;
                }

                if (result is StateHandleResult<TStateBase, TResult, TError>.FinishedWithResult finishedWithResult)
                {
                    context.ExecutionMetadata.UpdateState(finishedWithResult.State);
                    return finishedWithResult.Result;
                }

                throw new InvalidOperationException($"Invalid state handler result = {result}");
            }

            if (handled is false)
            {
                throw new InvalidOperationException(
                    $"Could not find handler for state = {context.ExecutionMetadata.State}");
            }
        }
    }
}