using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;

namespace Itmo.Dev.Platform.BackgroundTasks.Managing.Running;

internal class ExecutionMetadataConfigurator<TMetadata> : IExecutionMetadataConfigurator<TMetadata>
    where TMetadata : IBackgroundTaskMetadata
{
    private readonly BackgroundTaskRunner _runner;
    private readonly TMetadata _metadata;

    public ExecutionMetadataConfigurator(BackgroundTaskRunner runner, TMetadata metadata)
    {
        _runner = runner;
        _metadata = metadata;
    }

    public IRunTaskRequest<TMetadata, T> WithExecutionMetadata<T>(T executionMetadata)
        where T : IBackgroundTaskExecutionMetadata
    {
        return new RunTaskRequest<TMetadata, T>(_runner, _metadata, executionMetadata);
    }
}