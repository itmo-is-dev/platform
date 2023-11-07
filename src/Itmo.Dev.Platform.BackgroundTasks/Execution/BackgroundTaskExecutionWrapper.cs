using Itmo.Dev.Platform.BackgroundTasks.Configuration;
using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using Itmo.Dev.Platform.BackgroundTasks.Registry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using BackgroundTaskQuery = Itmo.Dev.Platform.BackgroundTasks.Models.BackgroundTaskQuery;

namespace Itmo.Dev.Platform.BackgroundTasks.Execution;

internal class BackgroundTaskExecutionWrapper : IBackgroundTaskManager
{
    private readonly IServiceProvider _provider;
    private readonly IBackgroundTaskRegistry _taskRegistry;
    private readonly IBackgroundTaskInfrastructureRepository _repository;
    private readonly BackgroundTaskExecutionOptions _options;

    public BackgroundTaskExecutionWrapper(
        IServiceProvider provider,
        IBackgroundTaskRegistry taskRegistry,
        IBackgroundTaskInfrastructureRepository repository,
        IOptions<BackgroundTaskExecutionOptions> options)
    {
        _provider = provider;
        _taskRegistry = taskRegistry;
        _repository = repository;
        _options = options.Value;
    }

    public async Task ExecuteAsync(BackgroundTaskId id, CancellationToken cancellationToken)
    {
        var query = BackgroundTaskQuery.Build(builder => builder.WithId(id));
        var backgroundTask = await _repository.QueryAsync(query, cancellationToken).SingleAsync(cancellationToken);

        var registryRecord = _taskRegistry[backgroundTask.Name];

        var executorType = typeof(BackgroundTaskExecutor<,,,,>).MakeGenericType(
            registryRecord.TaskType,
            registryRecord.MetadataType,
            registryRecord.ExecutionMetadataType,
            registryRecord.ResultType,
            registryRecord.ErrorType);

        var executor = (IBackgroundTaskInternalExecutor)ActivatorUtilities.CreateInstance(_provider, executorType);
        await executor.ExecuteAsync(backgroundTask, cancellationToken);
    }

    public async Task FailedAsync(BackgroundTaskId id, CancellationToken cancellationToken)
    {
        var query = BackgroundTaskQuery.Build(builder => builder.WithId(id));
        var backgroundTask = await _repository.QueryAsync(query, cancellationToken).SingleAsync(cancellationToken);

        backgroundTask = backgroundTask with
        {
            State = backgroundTask.RetryNumber < _options.MaxRetryCount
                ? BackgroundTaskState.Retrying
                : BackgroundTaskState.Failed,

            RetryNumber = backgroundTask.RetryNumber + 1,
        };

        await _repository.UpdateAsync(backgroundTask, cancellationToken);
    }
}