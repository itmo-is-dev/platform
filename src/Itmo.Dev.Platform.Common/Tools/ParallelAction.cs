namespace Itmo.Dev.Platform.Common.Tools;

internal sealed class ParallelAction
{
    private readonly Func<CancellationToken, Task> _action;
    private readonly int _parallelism;

    public ParallelAction(int parallelism, Func<CancellationToken, Task> action)
    {
        if (parallelism < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(parallelism),
                parallelism,
                "Parallelism value must be at least 1");
        }

        _action = action;
        _parallelism = parallelism;
    }

    public static Task ExecuteAsync(CancellationToken cancellationToken, params ParallelAction[] actions)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var tasks = new Task[actions.Sum(x => x._parallelism)];
        var index = 0;

        foreach (var action in actions)
        {
            for (var i = 0; i < action._parallelism; i++)
            {
                tasks[index + i] = action.ExecuteSingleAsync(cts);
            }

            index += action._parallelism;
        }

        return ExecuteAllAsync(cts, tasks);

        static async Task ExecuteAllAsync(CancellationTokenSource cts, IEnumerable<Task> tasks)
        {
            using var source = cts;
            await Task.WhenAll(tasks);
        }
    }

    private async Task ExecuteSingleAsync(CancellationTokenSource cts)
    {
        cts.Token.ThrowIfCancellationRequested();

        try
        {
            await _action.Invoke(cts.Token);
        }
        catch
        {
            cts.Cancel();
            throw;
        }
    }
}