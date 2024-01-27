using Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask.Models;
using Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask.States;

namespace Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask.StateHandlers;

public class WaitingFirstStateHandler : IStateHandler<WaitingFirstState>
{
    public ValueTask<StateHandlerResult> HandleAsync(
        WaitingFirstState state,
        StateHandlerContext context,
        CancellationToken cancellationToken)
    {
        var result = new StateHandlerResult.Finished(new FirstState());
        return ValueTask.FromResult<StateHandlerResult>(result);
    }
}