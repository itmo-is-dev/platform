using Itmo.Dev.Platform.BackgroundTasks.StateMachine;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldExecuteStateMachine_WhenItContainsSuspends.
    States;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.
    RunWithAsync_ShouldExecuteStateMachine_WhenItContainsSuspends;

public class SuspendedTaskExecutionMetadata : IStateExecutionMetadata<SuspendedTaskState>
{
    public SuspendedTaskExecutionMetadata(SuspendedTaskState? state = null)
    {
        State = state ?? new StartingSuspendedTaskState();
    }

    public SuspendedTaskState State { get; set; }

    public void UpdateState(SuspendedTaskState state)
    {
        State = state;
    }
}