using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.BackgroundTasks.Configuration;

public interface IBackgroundTaskPersistenceConfigurationSelector
{
    IBackgroundTaskSchedulingOptionsConfigurator UsePersistenceConfigurator(
        IBackgroundTasksPersistenceConfigurator configurator);
}

public interface IBackgroundTaskSchedulingOptionsConfigurator
{
    IBackgroundTaskSchedulingConfigurationSelector ConfigureScheduling(
        Action<OptionsBuilder<BackgroundTaskSchedulingOptions>> configuration);
}

public interface IBackgroundTaskSchedulingConfigurationSelector
{
    IBackgroundTaskExecutionConfigurator UseSchedulingConfigurator(IBackgroundTaskSchedulingConfigurator configurator);
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