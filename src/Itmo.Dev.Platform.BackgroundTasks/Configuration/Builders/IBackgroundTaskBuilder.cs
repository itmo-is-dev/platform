using Itmo.Dev.Platform.BackgroundTasks.Registry;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.Configuration;

public interface IBackgroundTaskMetadataConfigurator
{
    IBackgroundTaskExecutionMetadataConfigurator<T> WithMetadata<T>()
        where T : IBackgroundTaskMetadata;
}

public interface IBackgroundTaskExecutionMetadataConfigurator<TMetadata>
    where TMetadata : IBackgroundTaskMetadata
{
    IBackgroundTaskResultConfigurator<TMetadata, T> WithExecutionMetadata<T>()
        where T : IBackgroundTaskExecutionMetadata;
}

public interface IBackgroundTaskResultConfigurator<TMetadata, TExecutionMetadata>
    where TMetadata : IBackgroundTaskMetadata
    where TExecutionMetadata : IBackgroundTaskExecutionMetadata
{
    IBackgroundTaskErrorConfigurator<TMetadata, TExecutionMetadata, T> WithResult<T>()
        where T : IBackgroundTaskResult;
}

public interface IBackgroundTaskErrorConfigurator<TMetadata, TExecutionMetadata, TResult>
    where TMetadata : IBackgroundTaskMetadata
    where TExecutionMetadata : IBackgroundTaskExecutionMetadata
    where TResult : IBackgroundTaskResult
{
    IBackgroundTaskConfigurator<TMetadata, TExecutionMetadata, TResult, T> WithError<T>()
        where T : IBackgroundTaskError;
}

public interface IBackgroundTaskConfigurator<TMetadata, TExecutionMetadata, TResult, TError>
    where TMetadata : IBackgroundTaskMetadata
    where TExecutionMetadata : IBackgroundTaskExecutionMetadata
    where TResult : IBackgroundTaskResult
    where TError : IBackgroundTaskError
{
    IBackgroundTaskBuilder<T, TMetadata, TExecutionMetadata, TResult, TError> HandleBy<T>()
        where T : class, IBackgroundTask<TMetadata, TExecutionMetadata, TResult, TError>;
}

public interface IBackgroundTaskBuilder<TTask, TMetadata, TExecutionMetadata, TResult, TError>
    where TTask : IBackgroundTask<TMetadata, TExecutionMetadata, TResult, TError>
    where TMetadata : IBackgroundTaskMetadata
    where TExecutionMetadata : IBackgroundTaskExecutionMetadata
    where TResult : IBackgroundTaskResult
    where TError : IBackgroundTaskError
{
    BackgroundTaskRegistryRecord Build();
}