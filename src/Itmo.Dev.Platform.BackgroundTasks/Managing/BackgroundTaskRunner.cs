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

    private async Task<BackgroundTaskId> RunTaskAsync<TTask, TMetadata, TExecutionMetadata>(
        TMetadata metadata,
        TExecutionMetadata executionMetadata,
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

    private class MetadataConfigurator : IMetadataConfigurator
    {
        private readonly BackgroundTaskRunner _runner;

        public MetadataConfigurator(BackgroundTaskRunner runner)
        {
            _runner = runner;
        }

        public IExecutionMetadataConfigurator<T> WithMetadata<T>(T metadata) where T : IBackgroundTaskMetadata
        {
            return new ExecutionMetadataConfigurator<T>(_runner, metadata);
        }
    }

    private class ExecutionMetadataConfigurator<TMetadata> : IExecutionMetadataConfigurator<TMetadata>
        where TMetadata : IBackgroundTaskMetadata
    {
        private readonly BackgroundTaskRunner _runner;
        private readonly TMetadata _metadata;

        public ExecutionMetadataConfigurator(BackgroundTaskRunner runner, TMetadata metadata)
        {
            _runner = runner;
            _metadata = metadata;
        }

        public IRunTaskRequest<TMetadata, T> WithExecutionMetadata<T>(T executionMetadata)
            where T : IBackgroundTaskExecutionMetadata
        {
            return new RunTaskRequest<TMetadata, T>(_runner, _metadata, executionMetadata);
        }
    }

    private class RunTaskRequest<TMetadata, TExecutionMetadata> : IRunTaskRequest<TMetadata, TExecutionMetadata>
        where TMetadata : IBackgroundTaskMetadata
        where TExecutionMetadata : IBackgroundTaskExecutionMetadata
    {
        private readonly BackgroundTaskRunner _runner;
        private readonly TMetadata _metadata;
        private readonly TExecutionMetadata _executionMetadata;

        public RunTaskRequest(BackgroundTaskRunner runner, TMetadata metadata, TExecutionMetadata executionMetadata)
        {
            _runner = runner;
            _metadata = metadata;
            _executionMetadata = executionMetadata;
        }

        public Task<BackgroundTaskId> RunWithAsync<TTask>(CancellationToken cancellationToken)
            where TTask : IBackgroundTask<TMetadata, TExecutionMetadata>
        {
            return _runner.RunTaskAsync<TTask, TMetadata, TExecutionMetadata>(
                _metadata,
                _executionMetadata,
                cancellationToken);
        }
    }
}