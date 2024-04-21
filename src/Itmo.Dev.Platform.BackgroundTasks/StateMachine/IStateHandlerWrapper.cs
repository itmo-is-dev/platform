using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.StateMachine;

public interface IStateHandlerWrapper<TStateBase, TMetadata, TExecutionMetadata, TResult, TError>
    where TStateBase : IState
    where TMetadata : IBackgroundTaskMetadata
    where TExecutionMetadata : IStateExecutionMetadata<TStateBase>
    where TResult : IBackgroundTaskResult
    where TError : IBackgroundTaskError
{
    ValueTask<StateHandleResult<TStateBase, TResult, TError>?> TryHandleAsync(
        TStateBase state,
        BackgroundTaskExecutionContext<TMetadata, TExecutionMetadata> context,
        CancellationToken cancellationToken);
}