using Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask.Models;
using Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask.States;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;

namespace Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask.StateHandlers;

public class CompletedStateHandler : IStateHandler<CompletedState>
{
    public ValueTask<StateHandlerResult> HandleAsync(
        CompletedState state,
        StateHandlerContext context,
        CancellationToken cancellationToken)
    {
        var error = new StateTaskError("Task is in completed state");

        var result = new StateHandlerResult.FinishedWithResult(
            state,
            BackgroundTaskExecutionResult.Failure.ForEmptyResult().WithError(error));

        return ValueTask.FromResult<StateHandlerResult>(result);
    }
}