using Itmo.Dev.Platform.BackgroundTasks.StateMachine;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldExecuteStateMachine_WhenItContainsSuspends.
    States;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldExecuteStateMachine_WhenItContainsSuspends
    .StateHandlers;

public interface ISuspendedTaskStateHandler<in TState> : IStateHandler<
    TState, 
    SuspendedTaskState,
    EmptyMetadata,
    SuspendedTaskExecutionMetadata,
    EmptyExecutionResult,
    EmptyError>
    where TState : SuspendedTaskState;