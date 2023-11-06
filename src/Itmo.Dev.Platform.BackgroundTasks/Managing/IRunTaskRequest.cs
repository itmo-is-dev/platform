using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;

namespace Itmo.Dev.Platform.BackgroundTasks.Managing;

public interface IMetadataConfigurator
{
    IRunTaskRequest<T> WithMetadata<T>(T metadata) where T : IBackgroundTaskMetadata;
}

public interface IRunTaskRequest<TMetadata> where TMetadata : IBackgroundTaskMetadata
{
    Task<BackgroundTaskId> RunWithAsync<TTask>(CancellationToken cancellationToken)
        where TTask : IBackgroundTask<TMetadata>;
}