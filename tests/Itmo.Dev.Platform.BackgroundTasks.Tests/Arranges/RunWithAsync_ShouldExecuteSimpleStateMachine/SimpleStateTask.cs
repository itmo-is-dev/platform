using Itmo.Dev.Platform.BackgroundTasks.StateMachine;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldExecuteSimpleStateMachine.StateHandlers;
using Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldExecuteSimpleStateMachine.States;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldExecuteSimpleStateMachine;

public class SimpleStateTask : IBackgroundTask<
    EmptyMetadata,
    SimpleStateExecutionMetadata,
    EmptyExecutionResult,
    EmptyError>
{
    private readonly IStateMachineFactory _stateMachineFactory;

    public SimpleStateTask(IStateMachineFactory stateMachineFactory)
    {
        _stateMachineFactory = stateMachineFactory;
    }

    public static string Name => nameof(SimpleStateTask);

    public async Task<BackgroundTaskExecutionResult<EmptyExecutionResult, EmptyError>> ExecuteAsync(
        BackgroundTaskExecutionContext<EmptyMetadata, SimpleStateExecutionMetadata> executionContext,
        CancellationToken cancellationToken)
    {
        return await _stateMachineFactory
            .CreateForState<SimpleState>()
            .ForEmptyMetadata()
            .ForExecutionMetadata<SimpleStateExecutionMetadata>()
            .ForEmptyResult()
            .ForEmptyError()
            .WithState<StartingSimpleState, StartingSimpleStateHandler>()
            .WithState<FirstSimpleState, FirstSimpleStateHandler>()
            .WithState<SecondSimpleState, SecondSimpleStateHandler>()
            .Build()
            .RunAsync(executionContext, cancellationToken);
    }
}