using Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask.States;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask.Models;

public abstract record StateHandlerResult
{
    private StateHandlerResult() { }

    public sealed record Finished(TaskState State) : StateHandlerResult;

    public sealed record FinishedWithResult(
        TaskState State,
        BackgroundTaskExecutionResult<EmptyExecutionResult, StateTaskError> Result) : StateHandlerResult;
}