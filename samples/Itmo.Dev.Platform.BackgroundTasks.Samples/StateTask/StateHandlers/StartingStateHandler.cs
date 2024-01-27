using Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask.Models;
using Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask.States;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;

namespace Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask.StateHandlers;

public class StartingStateHandler : IStateHandler<StartingState>
{
    public ValueTask<StateHandlerResult> HandleAsync(
        StartingState state,
        StateHandlerContext context,
        CancellationToken cancellationToken)
    {
        var result = new StateHandlerResult.FinishedWithResult(
            new FirstState(),
            BackgroundTaskExecutionResult.Suspended.ForEmptyResult().ForError<StateTaskError>());

        return ValueTask.FromResult<StateHandlerResult>(result);
    }
}