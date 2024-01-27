using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;

namespace Itmo.Dev.Platform.BackgroundTasks.Samples.SuspendedTask;

public class SuspendedTaskExecutionMetadata : IBackgroundTaskExecutionMetadata
{
    public int Count { get; set; }
}