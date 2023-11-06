using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Microsoft.Extensions.Configuration;

namespace Itmo.Dev.Platform.BackgroundTasks.Configuration.Builders;

public interface IBackgroundTaskPersistenceConfigurator
{
    IBackgroundTaskSchedulingConfigurator ConfigurePersistence(
        IConfiguration configuration,
        Action<BackgroundTaskPersistenceOptions>? action = null);
}

public interface IBackgroundTaskSchedulingConfigurator
{
    IBackgroundTaskExecutionConfigurator ConfigureScheduling(
        IConfiguration configuration,
        Action<BackgroundTaskSchedulingOptions>? action = null);
}

public interface IBackgroundTaskExecutionConfigurator
{
    IBackgroundTaskConfigurationBuilder ConfigureExecution(
        IConfiguration configuration,
        Action<BackgroundTaskExecutionOptions>? action = null);
}

public interface IBackgroundTaskConfigurationBuilder
{
    IBackgroundTaskConfigurationBuilder AddBackgroundTask<TTask, TMetadata, TResult, TError>(
        Func<IBackgroundTaskMetadataConfigurator, IBackgroundTaskBuilder<TTask, TMetadata, TResult, TError>> func)
        where TTask : class, IBackgroundTask<TMetadata, TResult, TError>
        where TMetadata : IBackgroundTaskMetadata
        where TResult : IBackgroundTaskResult
        where TError : IBackgroundTaskError;
}