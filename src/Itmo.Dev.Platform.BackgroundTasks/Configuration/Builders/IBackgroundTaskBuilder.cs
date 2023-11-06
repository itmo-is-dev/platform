using Itmo.Dev.Platform.BackgroundTasks.Registry;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.Configuration.Builders;

public interface IBackgroundTaskMetadataConfigurator
{
    IBackgroundTaskResultConfigurator<T> WithMetadata<T>()
        where T : IBackgroundTaskMetadata;
}

public interface IBackgroundTaskResultConfigurator<TMetadata>
    where TMetadata : IBackgroundTaskMetadata
{
    IBackgroundTaskErrorConfigurator<TMetadata, T> WithResult<T>()
        where T : IBackgroundTaskResult;
}

public interface IBackgroundTaskErrorConfigurator<TMetadata, TResult>
    where TMetadata : IBackgroundTaskMetadata
    where TResult : IBackgroundTaskResult
{
    IBackgroundTaskConfigurator<TMetadata, TResult, T> WithError<T>()
        where T : IBackgroundTaskError;
}

public interface IBackgroundTaskConfigurator<TMetadata, TResult, TError>
    where TMetadata : IBackgroundTaskMetadata
    where TResult : IBackgroundTaskResult
    where TError : IBackgroundTaskError
{
    IBackgroundTaskBuilder<T, TMetadata, TResult, TError> HandleBy<T>()
        where T : class, IBackgroundTask<TMetadata, TResult, TError>;
}

public interface IBackgroundTaskBuilder<TTask, TMetadata, TResult, TError>
    where TTask : IBackgroundTask<TMetadata, TResult, TError>
    where TMetadata : IBackgroundTaskMetadata
    where TResult : IBackgroundTaskResult
    where TError : IBackgroundTaskError
{
    BackgroundTaskRegistryRecord Build();
}