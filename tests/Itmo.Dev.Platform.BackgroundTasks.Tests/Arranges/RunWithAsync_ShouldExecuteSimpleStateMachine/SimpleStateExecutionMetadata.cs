using Itmo.Dev.Platform.BackgroundTasks.StateMachine;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldExecuteSimpleStateMachine.States;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldExecuteSimpleStateMachine;

public class SimpleStateExecutionMetadata : IStateExecutionMetadata<SimpleState>
{
    public SimpleStateExecutionMetadata(SimpleState? state = null)
    {
        State = state ?? new StartingSimpleState();
    }

    public SimpleState State { get; set; }

    public void UpdateState(SimpleState state)
    {
        State = state;
    }
}