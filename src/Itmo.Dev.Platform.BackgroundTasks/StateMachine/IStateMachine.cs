using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.StateMachine;

public interface IStateMachine<TStateBase, TMetadata, TExecutionMetadata, TResult, TError>
    where TStateBase : IState
    where TMetadata : IBackgroundTaskMetadata
    where TExecutionMetadata : IStateExecutionMetadata<TStateBase>
    where TResult : IBackgroundTaskResult
    where TError : IBackgroundTaskError
{
    ValueTask<BackgroundTaskExecutionResult<TResult, TError>> RunAsync(
        BackgroundTaskExecutionContext<TMetadata, TExecutionMetadata> context,
        CancellationToken cancellationToken);
}