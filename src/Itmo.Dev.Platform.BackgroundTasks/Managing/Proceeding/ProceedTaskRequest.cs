using Itmo.Dev.Platform.BackgroundTasks.Models;

namespace Itmo.Dev.Platform.BackgroundTasks.Managing.Proceeding;

internal class ProceedTaskRequest : IProceedTaskRequest
{
    private readonly BackgroundTaskRunner _runner;
    private readonly BackgroundTaskQuery _query;
    private readonly ExecutionMetadataModification _executionMetadataModification;

    public ProceedTaskRequest(
        BackgroundTaskRunner runner,
        BackgroundTaskQuery query,
        ExecutionMetadataModification executionMetadataModification)
    {
        _runner = runner;
        _query = query;
        _executionMetadataModification = executionMetadataModification;
    }

    public Task<ProceedTaskResult> ProceedAsync(CancellationToken cancellationToken)
    {
        return _runner.ProceedAsync(_query, _executionMetadataModification, scheduledAt: null, cancellationToken);
    }

    public Task<ProceedTaskResult> ProceedAtAsync(DateTimeOffset scheduledAt, CancellationToken cancellationToken)
    {
        return _runner.ProceedAsync(_query,
            _executionMetadataModification,
            scheduledAt: scheduledAt,
            cancellationToken);
    }
}
