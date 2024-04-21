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

namespace Itmo.Dev.Platform.BackgroundTasks.Configuration.Builders;

internal class BackgroundTaskConfigurationBuilder :
    IBackgroundTaskPersistenceConfigurator,
    IBackgroundTaskSchedulingConfigurator,
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

    public IBackgroundTaskSchedulingConfigurator ConfigurePersistence(
        IConfiguration configuration,
        Action<BackgroundTaskPersistenceOptions>? action = null)
    {
        var builder = _collection.AddOptions<BackgroundTaskPersistenceOptions>().Bind(configuration);

        if (action is not null)
            builder.Configure(action);

        return this;
    }

    public IBackgroundTaskExecutionConfigurator ConfigureScheduling(
        IConfiguration configuration,
        Action<BackgroundTaskSchedulingOptions>? action = null)
    {
        var builder = _collection.AddOptions<BackgroundTaskSchedulingOptions>().Bind(configuration);

        if (action is not null)
            builder.Configure(action);

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