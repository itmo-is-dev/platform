using Itmo.Dev.Platform.BackgroundTasks.Managing;
using Itmo.Dev.Platform.BackgroundTasks.Models;

namespace Itmo.Dev.Platform.BackgroundTasks.Samples.SuspendedTask;

public class UsageScenario
{
    private readonly IBackgroundTaskRunner _runner;

    public UsageScenario(IBackgroundTaskRunner runner)
    {
        _runner = runner;
    }

    public Task<BackgroundTaskId> StartAsync(Guid counterId, CancellationToken cancellationToken)
    {
        return _runner
            .StartBackgroundTask
            .WithMetadata(new SuspendedTaskMetadata(counterId))
            .WithExecutionMetadata(new SuspendedTaskExecutionMetadata())
            .RunWithAsync<SuspendedBackgroundTask>(cancellationToken);
    }

    public async Task ProceedAsync(Guid counterId, CancellationToken cancellationToken)
    {
        var query = BackgroundTaskQuery.Build(x => x
            .WithName(SuspendedBackgroundTask.Name)
            .WithState(BackgroundTaskState.Suspended)
            .WithMetadata(new SuspendedTaskMetadata(counterId)));

        await _runner
            .ProceedBackgroundTask
            .WithQuery(query)
            .WithoutExecutionMetadataModification()
            .ProceedAsync(cancellationToken);
    }
}