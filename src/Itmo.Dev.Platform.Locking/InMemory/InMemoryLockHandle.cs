namespace Itmo.Dev.Platform.Locking.InMemory;

internal class InMemoryLockHandle : ILockHandle
{
    private readonly SemaphoreWrapper _semaphore;

    public InMemoryLockHandle(SemaphoreWrapper semaphore)
    {
        _semaphore = semaphore;
    }

    public ValueTask DisposeAsync()
    {
        _semaphore.Semaphore.Release();
        return ValueTask.CompletedTask;
    }
}