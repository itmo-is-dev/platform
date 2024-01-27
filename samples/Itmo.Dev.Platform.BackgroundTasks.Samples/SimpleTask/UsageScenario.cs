using Itmo.Dev.Platform.BackgroundTasks.Managing;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;

namespace Itmo.Dev.Platform.BackgroundTasks.Samples.SimpleTask;

public class UsageScenario
{
    private readonly IBackgroundTaskRunner _runner;

    public UsageScenario(IBackgroundTaskRunner runner)
    {
        _runner = runner;
    }

    public async Task StartAsync(string[] values, CancellationToken cancellationToken)
    {
        await _runner
            .StartBackgroundTask
            .WithMetadata(new SimpleTaskMetadata(values))
            .WithExecutionMetadata(EmptyExecutionMetadata.Value)
            .RunWithAsync<SimpleBackgroundTask>(cancellationToken);
    }
}