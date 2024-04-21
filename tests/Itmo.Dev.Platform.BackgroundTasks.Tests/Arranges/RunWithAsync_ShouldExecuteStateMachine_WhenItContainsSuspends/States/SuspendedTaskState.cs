using Itmo.Dev.Platform.BackgroundTasks.StateMachine;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldExecuteStateMachine_WhenItContainsSuspends.States;

public abstract record SuspendedTaskState : IState;