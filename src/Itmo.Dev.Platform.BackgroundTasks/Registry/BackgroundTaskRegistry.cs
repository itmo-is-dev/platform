namespace Itmo.Dev.Platform.BackgroundTasks.Registry;

internal class BackgroundTaskRegistry : IBackgroundTaskRegistry
{
    private readonly Dictionary<string, BackgroundTaskRegistryRecord> _dictionary;

    public BackgroundTaskRegistry()
    {
        _dictionary = new Dictionary<string, BackgroundTaskRegistryRecord>();
    }

    public BackgroundTaskRegistryRecord this[string backgroundTaskName] => _dictionary[backgroundTaskName];

    internal void AddRecord(BackgroundTaskRegistryRecord record)
    {
        _dictionary[record.Name] = record;
    }
}