using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;

namespace Itmo.Dev.Platform.BackgroundTasks.Managing.Running;

internal class RunTaskRequest<TMetadata, TExecutionMetadata> : IRunTaskRequest<TMetadata, TExecutionMetadata>
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
        return _runner.CreateBackgroundTaskAsync<TTask, TMetadata, TExecutionMetadata>(
            _metadata,
            _executionMetadata,
            scheduledAt: null,
            cancellationToken);
    }

    public Task<BackgroundTaskId> ScheduleWithAsync<TTask>(
        DateTimeOffset scheduledAt,
        CancellationToken cancellationToken)
        where TTask : IBackgroundTask<TMetadata, TExecutionMetadata>
    {
        return _runner.CreateBackgroundTaskAsync<TTask, TMetadata, TExecutionMetadata>(
            _metadata,
            _executionMetadata,
            scheduledAt: scheduledAt,
            cancellationToken);
    }
}
