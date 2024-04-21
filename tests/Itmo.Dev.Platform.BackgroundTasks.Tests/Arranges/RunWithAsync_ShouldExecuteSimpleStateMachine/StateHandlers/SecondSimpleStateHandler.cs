using Itmo.Dev.Platform.BackgroundTasks.StateMachine;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldExecuteSimpleStateMachine.States;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldScheduleAndExecuteTask;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldExecuteSimpleStateMachine.StateHandlers;

public class SecondSimpleStateHandler : ISimpleStateHandler<SecondSimpleState>
{
    private readonly CompletionManager _completionManager;

    public SecondSimpleStateHandler(CompletionManager completionManager)
    {
        _completionManager = completionManager;
    }

    public ValueTask<StateHandleResult<SimpleState, EmptyExecutionResult, EmptyError>> HandleAsync(
        SecondSimpleState state,
        BackgroundTaskExecutionContext<EmptyMetadata, SimpleStateExecutionMetadata> context,
        CancellationToken cancellationToken)
    {
        _completionManager.Complete(string.Empty);

        return ValueTask.FromResult<StateHandleResult<SimpleState, EmptyExecutionResult, EmptyError>>(
            StateHandleResult.FinishedWithResult
                .ForState<SimpleState>()
                .WithValue(new CompletedSimpleState())
                .ForEmptyResult()
                .ForEmptyError()
                .WithExecutionResult(BackgroundTaskExecutionResult.Success.WithEmptyResult().ForEmptyError()));
    }
}