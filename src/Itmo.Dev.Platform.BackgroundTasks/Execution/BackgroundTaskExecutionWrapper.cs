using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using Itmo.Dev.Platform.BackgroundTasks.Registry;
using Microsoft.Extensions.DependencyInjection;
using BackgroundTaskQuery = Itmo.Dev.Platform.BackgroundTasks.Models.BackgroundTaskQuery;

namespace Itmo.Dev.Platform.BackgroundTasks.Execution;

internal class BackgroundTaskExecutionWrapper : IBackgroundTaskExecutor
{
    private readonly IServiceProvider _provider;
    private readonly IBackgroundTaskRegistry _taskRegistry;
    private readonly IBackgroundTaskInfrastructureRepository _repository;

    public BackgroundTaskExecutionWrapper(
        IServiceProvider provider,
        IBackgroundTaskRegistry taskRegistry,
        IBackgroundTaskInfrastructureRepository repository)
    {
        _provider = provider;
        _taskRegistry = taskRegistry;
        _repository = repository;
    }

    public async Task ExecuteAsync(BackgroundTaskId id, CancellationToken cancellationToken)
    {
        var query = BackgroundTaskQuery.Build(builder => builder.WithId(id));
        var backgroundTask = await _repository.QueryAsync(query, cancellationToken).SingleAsync(cancellationToken);

        var registryRecord = _taskRegistry[backgroundTask.Name];

        var executorType = typeof(BackgroundTaskExecutor<,,,>).MakeGenericType(
            registryRecord.TaskType,
            registryRecord.MetadataType,
            registryRecord.ResultType,
            registryRecord.ErrorType);

        var executor = (IBackgroundTaskInternalExecutor)ActivatorUtilities.CreateInstance(_provider, executorType);
        await executor.ExecuteAsync(backgroundTask, cancellationToken);
    }
}