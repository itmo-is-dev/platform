using Itmo.Dev.Platform.Common.Lifetime.Exceptions;

namespace Itmo.Dev.Platform.Common.Lifetime.Initializers;

public abstract class PlatformLifetimeInitializerBase : IPlatformLifetimeInitializer
{
    private readonly TaskCompletionSource _tcs = new TaskCompletionSource();
    private int _running;

    public Task WaitForCompletionAsync(CancellationToken cancellationToken)
        => _tcs.Task;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.CompareExchange(ref _running, 1, 0) != 0)
            throw PlatformInitializerException.InitializerCalledMultipleTimes(GetType());

        try
        {
            await ExecuteAsync(cancellationToken);
            _tcs.SetResult();
        }
        catch (Exception e)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _tcs.SetCanceled(cancellationToken);
            }
            else
            {
                _tcs.SetException(e);
            }

            throw;
        }
    }

    protected abstract Task ExecuteAsync(CancellationToken cancellationToken);
}