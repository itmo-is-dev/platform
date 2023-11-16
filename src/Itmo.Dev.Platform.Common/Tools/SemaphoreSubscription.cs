namespace Itmo.Dev.Platform.Common.Tools;

public readonly struct SemaphoreSubscription : IDisposable
{
    private readonly SemaphoreSlim _semaphore;

    public SemaphoreSubscription(SemaphoreSlim semaphore)
    {
        _semaphore = semaphore;
    }

    public void Dispose()
    {
        _semaphore.Release();
    }
}