namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.RunWithAsync_ShouldScheduleAndExecuteTask;

public class CompletionManager
{
    private TaskCompletionSource<string> _tcs;

    public CompletionManager()
    {
        _tcs = new TaskCompletionSource<string>();
        WaitTask = GetTask(_tcs);
    }

    public Task<string> WaitTask { get; private set; }
    
    public int Version { get; private set; }

    public void Complete(string value)
        => _tcs.SetResult(value);

    public void Reset()
    {
        _tcs = new TaskCompletionSource<string>();
        WaitTask = GetTask(_tcs);

        Version++;
    }

    private static Task<string> GetTask(TaskCompletionSource<string> tcs)
    {
        return Task.Run(async () =>
        {
            var value = await tcs.Task;
            await Task.Delay(TimeSpan.FromMilliseconds(1500));

            return value;
        });
    }
}