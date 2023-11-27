using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;

namespace Itmo.Dev.Platform.BackgroundTasks.Managing.Running;

internal class MetadataConfigurator : IMetadataConfigurator
{
    private readonly BackgroundTaskRunner _runner;

    public MetadataConfigurator(BackgroundTaskRunner runner)
    {
        _runner = runner;
    }

    public IExecutionMetadataConfigurator<T> WithMetadata<T>(T metadata) where T : IBackgroundTaskMetadata
    {
        return new ExecutionMetadataConfigurator<T>(_runner, metadata);
    }
}