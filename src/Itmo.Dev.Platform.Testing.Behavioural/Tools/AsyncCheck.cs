using System.Runtime.CompilerServices;

namespace Itmo.Dev.Platform.Testing.Behavioural.Tools;

public sealed class AsyncCheck
{
    private TimeSpan _timeout = Timeout.InfiniteTimeSpan;
    private string _errorMessage = "Check failed";
    private Func<CancellationToken, Task> _assert = _ => throw new InvalidOperationException("Assert is not specified");

    private AsyncCheck() { }

    public static AsyncCheck Run => new AsyncCheck();

    public TaskAwaiter GetAwaiter() => CheckAsync().GetAwaiter();

    public AsyncCheck WithTimeout(TimeSpan timeout)
    {
        _timeout = timeout;
        return this;
    }

    public AsyncCheck WithErrorMessage(string errorMessage)
    {
        _errorMessage = errorMessage;
        return this;
    }

    public AsyncCheck WithAssert(Func<Task> assert)
    {
        _assert = _ => assert();
        return this;
    }

    public AsyncCheck WithAssert(Func<CancellationToken, Task> assert)
    {
        _assert = assert;
        return this;
    }

    private async Task CheckAsync()
    {
        using var cts = new CancellationTokenSource(_timeout);

        while (cts.IsCancellationRequested is false)
        {
            try
            {
                await _assert(cts.Token);
                return;
            }
            catch
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100), CancellationToken.None);
            }
        }

        throw new FailedCheckException(_errorMessage);
    }
}
