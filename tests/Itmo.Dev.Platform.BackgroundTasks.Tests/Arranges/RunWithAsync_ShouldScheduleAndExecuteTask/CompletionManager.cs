namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldScheduleAndExecuteTask;

public class CompletionManager
{
    private readonly TaskCompletionSource<string> _tcs = new TaskCompletionSource<string>();

    public Task<string> Task => _tcs.Task;

    public void Complete(string value)
        => _tcs.SetResult(value);
}