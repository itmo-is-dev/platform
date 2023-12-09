namespace Itmo.Dev.Platform.Locking.InMemory;

/// <summary>
///     As ConditionalWeakTable and GC does not call Dispose when item is collected,
///     this wrapper is needed to ensure all handles are properly disposed.
/// </summary>
internal sealed class SemaphoreWrapper
{
    public SemaphoreWrapper(SemaphoreSlim semaphore)
    {
        Semaphore = semaphore;
    }

    public SemaphoreSlim Semaphore { get; }

    ~SemaphoreWrapper()
    {
        Semaphore.Dispose();
    }
}