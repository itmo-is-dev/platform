namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldScheduleAndExecuteTask;

public class CompletionManager
{
    private readonly TaskCompletionSource<string> _tcs;

    public CompletionManager()
    {
        _tcs = new TaskCompletionSource<string>();

        WaitTask = Task.Run(async () =>
        {
            var value = await _tcs.Task;
            await Task.Delay(TimeSpan.FromMilliseconds(1500));

            return value;
        });
    }

    public Task<string> WaitTask { get; }

    public void Complete(string value)
        => _tcs.SetResult(value);
}