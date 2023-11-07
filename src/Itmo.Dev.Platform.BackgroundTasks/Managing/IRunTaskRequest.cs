using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;

namespace Itmo.Dev.Platform.BackgroundTasks.Managing;

public interface IMetadataConfigurator
{
    IExecutionMetadataConfigurator<T> WithMetadata<T>(T metadata) where T : IBackgroundTaskMetadata;
}

public interface IExecutionMetadataConfigurator<TMetadata>
    where TMetadata : IBackgroundTaskMetadata
{
    IRunTaskRequest<TMetadata, T> WithExecutionMetadata<T>(T executionMetadata)
        where T : IBackgroundTaskExecutionMetadata;
}

public interface IRunTaskRequest<TMetadata, TExecutionMetadata>
    where TMetadata : IBackgroundTaskMetadata
    where TExecutionMetadata : IBackgroundTaskExecutionMetadata
{
    Task<BackgroundTaskId> RunWithAsync<TTask>(CancellationToken cancellationToken)
        where TTask : IBackgroundTask<TMetadata, TExecutionMetadata>;
}