using Itmo.Dev.Platform.BackgroundTasks.Registry;
using Itmo.Dev.Platform.BackgroundTasks.StateMachine;
using Itmo.Dev.Platform.BackgroundTasks.StateMachine.Implementation;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.BackgroundTasks.Configuration;

internal class BackgroundTaskConfigurationBuilder :
    IBackgroundTaskPersistenceConfigurationSelector,
    IBackgroundTaskSchedulingOptionsConfigurator,
    IBackgroundTaskSchedulingConfigurationSelector,
    IBackgroundTaskExecutionConfigurator,
    IBackgroundTaskConfigurationBuilder,
    IBackgroundTaskStateMachineConfigurator
{
    private readonly IServiceCollection _collection;
    private readonly BackgroundTaskRegistry _registry;

    public BackgroundTaskConfigurationBuilder(IServiceCollection collection)
    {
        _collection = collection;
        _registry = new BackgroundTaskRegistry();

        _collection.AddSingleton<IBackgroundTaskRegistry>(_registry);
    }

    public IBackgroundTaskSchedulingOptionsConfigurator UsePersistenceConfigurator(
        IBackgroundTasksPersistenceConfigurator configurator)
    {
        configurator.Apply(_collection);
        return this;
    }

    public IBackgroundTaskSchedulingConfigurationSelector ConfigureScheduling(
        Action<OptionsBuilder<BackgroundTaskSchedulingOptions>> configuration)
    {
        var builder = _collection
            .AddOptions<BackgroundTaskSchedulingOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        configuration.Invoke(builder);

        return this;
    }

    public IBackgroundTaskExecutionConfigurator UseSchedulingConfigurator(
        IBackgroundTaskSchedulingConfigurator configurator)
    {
        configurator.Apply(_collection);
        return this;
    }

    public IBackgroundTaskStateMachineConfigurator ConfigureExecution(
        IConfiguration configuration,
        Action<BackgroundTaskExecutionOptions>? action = null)
    {
        var builder = _collection.AddOptions<BackgroundTaskExecutionOptions>().Bind(configuration);

        if (action is not null)
            builder.Configure(action);

        return this;
    }

    public IBackgroundTaskConfigurationBuilder AddBackgroundTask<TTask, TMetadata, TExecutionMetadata, TResult, TError>(
        Func<IBackgroundTaskMetadataConfigurator, IBackgroundTaskBuilder<
            TTask, TMetadata, TExecutionMetadata, TResult, TError>> func)
        where TTask : class, IBackgroundTask<TMetadata, TExecutionMetadata, TResult, TError>
        where TMetadata : IBackgroundTaskMetadata
        where TExecutionMetadata : IBackgroundTaskExecutionMetadata
        where TResult : IBackgroundTaskResult
        where TError : IBackgroundTaskError
    {
        var record = func.Invoke(new BackgroundTaskMetadataConfigurator()).Build();
        _registry.AddRecord(record);

        _collection.AddScoped<TTask>();

        return this;
    }

    public IBackgroundTaskConfigurationBuilder AddStateMachine()
    {
        _collection.AddScoped<IStateMachineFactory, StateMachineFactory>();
        return this;
    }
}