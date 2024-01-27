using Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask.Models;
using Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask.States;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;

namespace Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask.StateHandlers;

public class FirstStateHandler : IStateHandler<FirstState>
{
    public ValueTask<StateHandlerResult> HandleAsync(
        FirstState state,
        StateHandlerContext context,
        CancellationToken cancellationToken)
    {
        var result = new StateHandlerResult.FinishedWithResult(
            new CompletedState(),
            BackgroundTaskExecutionResult.Success.WithEmptyResult().ForError<StateTaskError>());

        return ValueTask.FromResult<StateHandlerResult>(result);
    }
}