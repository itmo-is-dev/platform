using Itmo.Dev.Platform.BackgroundTasks.Registry;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.Configuration;

internal class BackgroundTaskMetadataConfigurator : IBackgroundTaskMetadataConfigurator
{
    public IBackgroundTaskExecutionMetadataConfigurator<T> WithMetadata<T>() where T : IBackgroundTaskMetadata
        => new BackgroundTaskExecutionMetadataConfigurator<T>();
}

file class BackgroundTaskExecutionMetadataConfigurator<TMetadata> :
    IBackgroundTaskExecutionMetadataConfigurator<TMetadata>
    where TMetadata : IBackgroundTaskMetadata
{
    public IBackgroundTaskResultConfigurator<TMetadata, T> WithExecutionMetadata<T>()
        where T : IBackgroundTaskExecutionMetadata
    {
        return new BackgroundTaskResultConfigurator<TMetadata, T>();
    }
}

file class BackgroundTaskResultConfigurator<TMetadata, TExecutionMetadata> :
    IBackgroundTaskResultConfigurator<TMetadata, TExecutionMetadata>
    where TMetadata : IBackgroundTaskMetadata
    where TExecutionMetadata : IBackgroundTaskExecutionMetadata
{
    public IBackgroundTaskErrorConfigurator<TMetadata, TExecutionMetadata, T> WithResult<T>()
        where T : IBackgroundTaskResult
        => new BackgroundTaskErrorConfigurator<TMetadata, TExecutionMetadata, T>();
}

file class BackgroundTaskErrorConfigurator<TMetadata, TExecutionMetadata, TResult> :
    IBackgroundTaskErrorConfigurator<TMetadata, TExecutionMetadata, TResult>
    where TMetadata : IBackgroundTaskMetadata
    where TExecutionMetadata : IBackgroundTaskExecutionMetadata
    where TResult : IBackgroundTaskResult
{
    public IBackgroundTaskConfigurator<TMetadata, TExecutionMetadata, TResult, T> WithError<T>()
        where T : IBackgroundTaskError
        => new BackgroundTaskConfigurator<TMetadata, TExecutionMetadata, TResult, T>();
}

file class BackgroundTaskConfigurator<TMetadata, TExecutionMetadata, TResult, TError>
    : IBackgroundTaskConfigurator<TMetadata, TExecutionMetadata, TResult, TError>
    where TMetadata : IBackgroundTaskMetadata
    where TExecutionMetadata : IBackgroundTaskExecutionMetadata
    where TResult : IBackgroundTaskResult
    where TError : IBackgroundTaskError
{
    public IBackgroundTaskBuilder<T, TMetadata, TExecutionMetadata, TResult, TError> HandleBy<T>()
        where T : class, IBackgroundTask<TMetadata, TExecutionMetadata, TResult, TError>
    {
        return new BackgroundTaskBuilder<T, TMetadata, TExecutionMetadata, TResult, TError>();
    }
}

file class BackgroundTaskBuilder<TTask, TMetadata, TExecutionMetadata, TResult, TError>
    : IBackgroundTaskBuilder<TTask, TMetadata, TExecutionMetadata, TResult, TError>
    where TTask : class, IBackgroundTask<TMetadata, TExecutionMetadata, TResult, TError>
    where TMetadata : IBackgroundTaskMetadata
    where TExecutionMetadata : IBackgroundTaskExecutionMetadata
    where TResult : IBackgroundTaskResult
    where TError : IBackgroundTaskError
{
    public BackgroundTaskRegistryRecord Build()
    {
        return new BackgroundTaskRegistryRecord(
            TTask.Name,
            typeof(TTask),
            typeof(TMetadata),
            typeof(TExecutionMetadata),
            typeof(TResult),
            typeof(TError));
    }
}