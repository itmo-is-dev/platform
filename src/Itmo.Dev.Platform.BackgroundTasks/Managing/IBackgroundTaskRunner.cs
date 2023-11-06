namespace Itmo.Dev.Platform.BackgroundTasks.Managing;

public interface IBackgroundTaskRunner
{
    IMetadataConfigurator StartBackgroundTask { get; }
}