using Itmo.Dev.Platform.BackgroundTasks.StateMachine;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldExecuteSimpleStateMachine.States;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldExecuteSimpleStateMachine.StateHandlers;

public interface ISimpleStateHandler<TState> : IStateHandler<
    TState,
    SimpleState,
    EmptyMetadata,
    SimpleStateExecutionMetadata,
    EmptyExecutionResult,
    EmptyError>
    where TState : SimpleState;