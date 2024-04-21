using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;
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
    IBackgroundTaskStateMachineConfigurator ConfigureExecution(
        IConfiguration configuration,
        Action<BackgroundTaskExecutionOptions>? action = null);
}

public interface IBackgroundTaskStateMachineConfigurator : IBackgroundTaskConfigurationBuilder
{
    IBackgroundTaskConfigurationBuilder AddStateMachine();
}

public interface IBackgroundTaskConfigurationBuilder
{
    IBackgroundTaskConfigurationBuilder AddBackgroundTask<TTask, TMetadata, TExecutionMetadata, TResult, TError>(
        Func<IBackgroundTaskMetadataConfigurator, IBackgroundTaskBuilder<
            TTask, TMetadata, TExecutionMetadata, TResult, TError>> func)
        where TTask : class, IBackgroundTask<TMetadata, TExecutionMetadata, TResult, TError>
        where TMetadata : IBackgroundTaskMetadata
        where TExecutionMetadata : IBackgroundTaskExecutionMetadata
        where TResult : IBackgroundTaskResult
        where TError : IBackgroundTaskError;
}