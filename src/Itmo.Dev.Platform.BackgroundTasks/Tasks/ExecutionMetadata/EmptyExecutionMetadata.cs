namespace Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;

public class EmptyExecutionMetadata : IBackgroundTaskExecutionMetadata
{
    private EmptyExecutionMetadata() { }

    public static readonly EmptyExecutionMetadata Value = new EmptyExecutionMetadata();
}