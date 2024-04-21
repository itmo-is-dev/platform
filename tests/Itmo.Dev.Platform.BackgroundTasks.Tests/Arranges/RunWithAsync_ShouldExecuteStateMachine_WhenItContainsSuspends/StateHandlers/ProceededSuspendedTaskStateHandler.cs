using Itmo.Dev.Platform.BackgroundTasks.StateMachine;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldExecuteStateMachine_WhenItContainsSuspends.
    States;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldScheduleAndExecuteTask;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldExecuteStateMachine_WhenItContainsSuspends
    .StateHandlers;

public class ProceededSuspendedTaskStateHandler : ISuspendedTaskStateHandler<ProceededSuspendedTaskState>
{
    private readonly CompletionManager _completionManager;

    public ProceededSuspendedTaskStateHandler(CompletionManager completionManager)
    {
        _completionManager = completionManager;
    }

    public ValueTask<StateHandleResult<SuspendedTaskState, EmptyExecutionResult, EmptyError>> HandleAsync(
        ProceededSuspendedTaskState state,
        BackgroundTaskExecutionContext<EmptyMetadata, SuspendedTaskExecutionMetadata> context,
        CancellationToken cancellationToken)
    {
        _completionManager.Complete(string.Empty);

        return ValueTask.FromResult<StateHandleResult<SuspendedTaskState, EmptyExecutionResult, EmptyError>>(
            StateHandleResult.FinishedWithResult
                .ForState<SuspendedTaskState>()
                .WithValue(new CompletedSuspendedTaskState())
                .ForEmptyResult()
                .ForEmptyError()
                .WithSuccessResult(EmptyExecutionResult.Value));
    }
}