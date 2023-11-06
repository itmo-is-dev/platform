using Itmo.Dev.Platform.BackgroundTasks.Registry;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.Configuration.Builders;

internal class BackgroundTaskMetadataConfigurator : IBackgroundTaskMetadataConfigurator
{
    public IBackgroundTaskResultConfigurator<T> WithMetadata<T>() where T : IBackgroundTaskMetadata
        => new BackgroundTaskResultConfigurator<T>();
}

internal class BackgroundTaskResultConfigurator<TMetadata> : IBackgroundTaskResultConfigurator<TMetadata>
    where TMetadata : IBackgroundTaskMetadata
{
    public IBackgroundTaskErrorConfigurator<TMetadata, T> WithResult<T>() where T : IBackgroundTaskResult
        => new BackgroundTaskErrorConfigurator<TMetadata, T>();
}

internal class BackgroundTaskErrorConfigurator<TMetadata, TResult> : IBackgroundTaskErrorConfigurator<TMetadata, TResult>
    where TMetadata : IBackgroundTaskMetadata
    where TResult : IBackgroundTaskResult
{
    public IBackgroundTaskConfigurator<TMetadata, TResult, T> WithError<T>() where T : IBackgroundTaskError
        => new BackgroundTaskConfigurator<TMetadata, TResult, T>();
}

internal class BackgroundTaskConfigurator<TMetadata, TResult, TError>
    : IBackgroundTaskConfigurator<TMetadata, TResult, TError>
    where TMetadata : IBackgroundTaskMetadata
    where TResult : IBackgroundTaskResult
    where TError : IBackgroundTaskError
{
    public IBackgroundTaskBuilder<T, TMetadata, TResult, TError> HandleBy<T>()
        where T : class, IBackgroundTask<TMetadata, TResult, TError>
    {
        return new BackgroundTaskBuilder<T, TMetadata, TResult, TError>();
    }
}

internal class BackgroundTaskBuilder<TTask, TMetadata, TResult, TError>
    : IBackgroundTaskBuilder<TTask, TMetadata, TResult, TError>
    where TTask : class, IBackgroundTask<TMetadata, TResult, TError>
    where TMetadata : IBackgroundTaskMetadata
    where TResult : IBackgroundTaskResult
    where TError : IBackgroundTaskError
{
    public BackgroundTaskRegistryRecord Build()
    {
        return new BackgroundTaskRegistryRecord(
            TTask.Name,
            typeof(TTask),
            typeof(TMetadata),
            typeof(TResult),
            typeof(TError));
    }
}