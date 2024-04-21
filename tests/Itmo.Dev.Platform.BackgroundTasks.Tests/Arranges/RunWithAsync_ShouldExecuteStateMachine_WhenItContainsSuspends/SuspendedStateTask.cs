using Itmo.Dev.Platform.BackgroundTasks.StateMachine;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldExecuteStateMachine_WhenItContainsSuspends.
    StateHandlers;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldExecuteStateMachine_WhenItContainsSuspends.
    States;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.
    RunWithAsync_ShouldExecuteStateMachine_WhenItContainsSuspends;

public class SuspendedStateTask : IBackgroundTask<
    EmptyMetadata,
    SuspendedTaskExecutionMetadata,
    EmptyExecutionResult,
    EmptyError>
{
    private readonly IStateMachineFactory _stateMachineFactory;

    public SuspendedStateTask(IStateMachineFactory stateMachineFactory)
    {
        _stateMachineFactory = stateMachineFactory;
    }

    public static string Name => nameof(SuspendedStateTask);

    public async Task<BackgroundTaskExecutionResult<EmptyExecutionResult, EmptyError>> ExecuteAsync(
        BackgroundTaskExecutionContext<EmptyMetadata, SuspendedTaskExecutionMetadata> executionContext,
        CancellationToken cancellationToken)
    {
        return await _stateMachineFactory
            .CreateForState<SuspendedTaskState>()
            .ForEmptyMetadata()
            .ForExecutionMetadata<SuspendedTaskExecutionMetadata>()
            .ForEmptyResult()
            .ForEmptyError()
            .WithState<StartingSuspendedTaskState, StartingSuspendedTaskStateHandler>()
            .WithState<SuspendingSuspendedTaskState, SuspendingSuspendedTaskStateHandler>()
            .WithState<ProceededSuspendedTaskState, ProceededSuspendedTaskStateHandler>()
            .Build()
            .RunAsync(executionContext, cancellationToken);
    }
}