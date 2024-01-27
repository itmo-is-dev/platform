using Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask.Models;
using Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask.States;

namespace Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask;

public interface IStateHandler<in TState> where TState : TaskState
{
    ValueTask<StateHandlerResult> HandleAsync(
        TState state,
        StateHandlerContext context,
        CancellationToken cancellationToken);
}