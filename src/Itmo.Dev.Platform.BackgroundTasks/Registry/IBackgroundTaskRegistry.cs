namespace Itmo.Dev.Platform.BackgroundTasks.Registry;

public interface IBackgroundTaskRegistry
{
    BackgroundTaskRegistryRecord this[string backgroundTaskName] { get; }
}