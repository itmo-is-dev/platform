using Itmo.Dev.Platform.BackgroundTasks.StateMachine;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldExecuteSimpleStateMachine.States;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldExecuteSimpleStateMachine.StateHandlers;

public class FirstSimpleStateHandler : ISimpleStateHandler<FirstSimpleState>
{
    public ValueTask<StateHandleResult<SimpleState, EmptyExecutionResult, EmptyError>> HandleAsync(
        FirstSimpleState state,
        BackgroundTaskExecutionContext<EmptyMetadata, SimpleStateExecutionMetadata> context,
        CancellationToken cancellationToken)
    {
        return ValueTask.FromResult<StateHandleResult<SimpleState, EmptyExecutionResult, EmptyError>>(
            StateHandleResult.Finished
                .ForState<SimpleState>()
                .WithValue(new SecondSimpleState())
                .ForEmptyResult()
                .ForEmptyError());
    }
}