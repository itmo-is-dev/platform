using Itmo.Dev.Platform.BackgroundTasks.StateMachine;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldExecuteStateMachine_WhenItContainsSuspends.
    States;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldExecuteStateMachine_WhenItContainsSuspends
    .StateHandlers;

public class StartingSuspendedTaskStateHandler : ISuspendedTaskStateHandler<StartingSuspendedTaskState>
{
    public ValueTask<StateHandleResult<SuspendedTaskState, EmptyExecutionResult, EmptyError>> HandleAsync(
        StartingSuspendedTaskState state,
        BackgroundTaskExecutionContext<EmptyMetadata, SuspendedTaskExecutionMetadata> context,
        CancellationToken cancellationToken)
    {
        return ValueTask.FromResult<StateHandleResult<SuspendedTaskState, EmptyExecutionResult, EmptyError>>(
            StateHandleResult.Finished
                .ForState<SuspendedTaskState>()
                .WithValue(new SuspendingSuspendedTaskState())
                .ForEmptyResult()
                .ForEmptyError());
    }
}