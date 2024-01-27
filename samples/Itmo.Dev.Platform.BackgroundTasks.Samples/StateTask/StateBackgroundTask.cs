using Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask.Models;
using Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask.StateHandlers;
using Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask.States;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask;

public class StateBackgroundTask : IBackgroundTask<
    StateTaskMetadata,
    StateTaskExecutionMetadata,
    EmptyExecutionResult,
    StateTaskError>
{
    private readonly IServiceProvider _serviceProvider;

    public StateBackgroundTask(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public static string Name => nameof(StateBackgroundTask);

    public async Task<BackgroundTaskExecutionResult<EmptyExecutionResult, StateTaskError>> ExecuteAsync(
        BackgroundTaskExecutionContext<StateTaskMetadata, StateTaskExecutionMetadata> executionContext,
        CancellationToken cancellationToken)
    {
        executionContext.ExecutionMetadata.State ??= new StartingState();

        var context = new StateHandlerContext(executionContext.Id, executionContext.Metadata);

        while (true)
        {
            var result = await HandleAsync(executionContext.ExecutionMetadata.State, context, cancellationToken);

            if (result is StateHandlerResult.Finished finished)
            {
                executionContext.ExecutionMetadata.State = finished.State;
                continue;
            }

            if (result is StateHandlerResult.FinishedWithResult finishedWithResult)
            {
                executionContext.ExecutionMetadata.State = finishedWithResult.State;
                return finishedWithResult.Result;
            }

            return BackgroundTaskExecutionResult
                .Failure
                .ForEmptyResult()
                .WithError(new StateTaskError($"Invalid handler result = {result}"));
        }
    }

    private ValueTask<StateHandlerResult> HandleAsync(
        TaskState taskState,
        StateHandlerContext context,
        CancellationToken cancellationToken)
    {
        return taskState switch
        {
            StartingState state => ActivatorUtilities
                .CreateInstance<StartingStateHandler>(_serviceProvider)
                .HandleAsync(state, context, cancellationToken),

            WaitingFirstState state => ActivatorUtilities
                .CreateInstance<WaitingFirstStateHandler>(_serviceProvider)
                .HandleAsync(state, context, cancellationToken),

            FirstState state => ActivatorUtilities
                .CreateInstance<FirstStateHandler>(_serviceProvider)
                .HandleAsync(state, context, cancellationToken),

            CompletedState state => ActivatorUtilities
                .CreateInstance<CompletedStateHandler>(_serviceProvider)
                .HandleAsync(state, context, cancellationToken),

            _ => throw new ArgumentOutOfRangeException(nameof(taskState), taskState, "Could not resolve state handler"),
        };
    }
}