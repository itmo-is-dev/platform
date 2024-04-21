using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;

namespace Itmo.Dev.Platform.BackgroundTasks.StateMachine;

public interface IStateExecutionMetadata<TStateBase> : IBackgroundTaskExecutionMetadata
    where TStateBase : IState
{
    TStateBase State { get; }

    void UpdateState(TStateBase state);
}