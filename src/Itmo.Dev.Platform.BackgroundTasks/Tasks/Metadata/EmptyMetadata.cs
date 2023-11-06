namespace Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;

public class EmptyMetadata : IBackgroundTaskMetadata
{
    private EmptyMetadata() { }

    public static readonly EmptyMetadata Value = new EmptyMetadata();
}