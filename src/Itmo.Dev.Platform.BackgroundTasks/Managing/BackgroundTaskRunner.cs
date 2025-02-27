using Itmo.Dev.Platform.BackgroundTasks.Managing.Proceeding;
using Itmo.Dev.Platform.BackgroundTasks.Managing.Running;
using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.Common.DateTime;

namespace Itmo.Dev.Platform.BackgroundTasks.Managing;

internal class BackgroundTaskRunner : IBackgroundTaskRunner
{
    private readonly IBackgroundTaskInfrastructureRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public BackgroundTaskRunner(IBackgroundTaskInfrastructureRepository repository, IDateTimeProvider dateTimeProvider)
    {
        _repository = repository;
        _dateTimeProvider = dateTimeProvider;
    }

    public IMetadataConfigurator StartBackgroundTask => new MetadataConfigurator(this);

    public IQueryParameterConfigurator ProceedBackgroundTask => new QueryParameterConfigurator(this);

    internal async Task<BackgroundTaskId> CreateBackgroundTaskAsync<TTask, TMetadata, TExecutionMetadata>(
        TMetadata metadata,
        TExecutionMetadata executionMetadata,
        DateTimeOffset? scheduledAt,
        CancellationToken cancellationToken)
        where TMetadata : IBackgroundTaskMetadata
        where TExecutionMetadata : IBackgroundTaskExecutionMetadata
        where TTask : IBackgroundTask<TMetadata, TExecutionMetadata>
    {
        var backgroundTask = new BackgroundTask(
            Id: new BackgroundTaskId(0),
            Name: TTask.Name,
            Type: typeof(TTask),
            CreatedAt: _dateTimeProvider.Current,
            ScheduledAt: scheduledAt,
            State: BackgroundTaskState.Pending,
            RetryNumber: 0,
            Metadata: metadata,
            ExecutionMetadata: executionMetadata,
            Result: null,
            Error: null);

        var id = await _repository
            .AddRangeAsync(new[] { backgroundTask }, cancellationToken)
            .SingleAsync(cancellationToken);

        return id;
    }

    internal async Task<ProceedTaskResult> ProceedAsync(
        BackgroundTaskQuery query,
        ExecutionMetadataModification modification,
        DateTimeOffset? scheduledAt,
        CancellationToken cancellationToken)
    {
        var backgroundTasks = await _repository
            .QueryAsync(query, cancellationToken)
            .ToArrayAsync(cancellationToken);

        if (backgroundTasks is not [var backgroundTask])
            return new ProceedTaskResult.MultipleTasksFound(backgroundTasks);

        var modificationResult = modification.Modify(backgroundTask.ExecutionMetadata);

        if (modificationResult is ExecutionMetadataModificationResult.Failure modificationFailure)
            return new ProceedTaskResult.ExecutionMetadataModificationFailure(modificationFailure.Message);

        if (modificationResult is not ExecutionMetadataModificationResult.Success modificationSuccess)
            throw new InvalidOperationException("Execution metadata modification yielded unexpected result");

        backgroundTask = backgroundTask with
        {
            State = BackgroundTaskState.Proceeded,
            ScheduledAt = scheduledAt,
            ExecutionMetadata = modificationSuccess.Metadata,
        };

        await _repository.UpdateAsync(backgroundTask, cancellationToken);

        return new ProceedTaskResult.Success(backgroundTask);
    }
}