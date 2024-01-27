using Itmo.Dev.Platform.BackgroundTasks.Managing;
using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask.States;

namespace Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask;

public class UsageScenario
{
    private readonly IBackgroundTaskRunner _runner;

    public UsageScenario(IBackgroundTaskRunner runner)
    {
        _runner = runner;
    }

    public async Task StartAsync(Guid operationId, CancellationToken cancellationToken)
    {
        await _runner
            .StartBackgroundTask
            .WithMetadata(new StateTaskMetadata(operationId))
            .WithExecutionMetadata(new StateTaskExecutionMetadata())
            .RunWithAsync<StateBackgroundTask>(cancellationToken);
    }

    public async Task ProceedFirstStateAsync(Guid operationId, CancellationToken cancellationToken)
    {
        var query = BackgroundTaskQuery.Build(builder => builder
            .WithName(StateBackgroundTask.Name)
            .WithState(BackgroundTaskState.Suspended)
            .WithMetadata(new StateTaskMetadata(operationId))
            .WithExecutionMetadata(new StateTaskExecutionMetadata { State = new WaitingFirstState() }));

        await _runner
            .ProceedBackgroundTask
            .WithQuery(query)
            .WithoutExecutionMetadataModification()
            .ProceedAsync(cancellationToken);
    }
}