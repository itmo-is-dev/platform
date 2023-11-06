using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
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

    private async Task<BackgroundTaskId> RunTaskAsync<TTask, TMetadata>(
        IBackgroundTaskMetadata metadata,
        CancellationToken cancellationToken)
        where TMetadata : IBackgroundTaskMetadata
        where TTask : IBackgroundTask<TMetadata>
    {
        var backgroundTask = new BackgroundTask(
            Id: new BackgroundTaskId(0),
            Name: TTask.Name,
            Type: typeof(TTask),
            CreatedAt: _dateTimeProvider.Current,
            State: BackgroundTaskState.Pending,
            RetryNumber: 0,
            Metadata: metadata,
            Result: null,
            Error: null);

        var id = await _repository
            .AddRangeAsync(new[] { backgroundTask }, cancellationToken)
            .SingleAsync(cancellationToken);

        return id;
    }

    private class MetadataConfigurator : IMetadataConfigurator
    {
        private readonly BackgroundTaskRunner _runner;

        public MetadataConfigurator(BackgroundTaskRunner runner)
        {
            _runner = runner;
        }

        public IRunTaskRequest<T> WithMetadata<T>(T metadata) where T : IBackgroundTaskMetadata
        {
            return new RunTaskRequest<T>(_runner, metadata);
        }
    }

    private class RunTaskRequest<TMetadata> : IRunTaskRequest<TMetadata>
        where TMetadata : IBackgroundTaskMetadata
    {
        private readonly BackgroundTaskRunner _runner;
        private readonly TMetadata _metadata;

        public RunTaskRequest(BackgroundTaskRunner runner, TMetadata metadata)
        {
            _runner = runner;
            _metadata = metadata;
        }

        public Task<BackgroundTaskId> RunWithAsync<TTask>(CancellationToken cancellationToken)
            where TTask : IBackgroundTask<TMetadata>
        {
            return _runner.RunTaskAsync<TTask, TMetadata>(_metadata, cancellationToken);
        }
    }
}