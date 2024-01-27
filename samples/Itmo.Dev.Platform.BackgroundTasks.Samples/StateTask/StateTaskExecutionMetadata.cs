using Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask.States;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;

namespace Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask;

public class StateTaskExecutionMetadata : IBackgroundTaskExecutionMetadata
{
    public TaskState? State { get; set; }
}