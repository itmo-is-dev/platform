using System.Runtime.CompilerServices;

namespace Itmo.Dev.Platform.Locking.InMemory;

internal class InMemoryLockingService : ILockingService
{
    private readonly ConditionalWeakTable<object, SemaphoreWrapper> _table;

    public InMemoryLockingService()
    {
        _table = new ConditionalWeakTable<object, SemaphoreWrapper>();
    }

    public async ValueTask<ILockHandle> AcquireAsync(object key, CancellationToken cancellationToken)
    {
        SemaphoreWrapper semaphore = _table.GetValue(
            key,
            _ => new SemaphoreWrapper(new SemaphoreSlim(1, 1)));

        await semaphore.Semaphore.WaitAsync(cancellationToken);

        return new InMemoryLockHandle(semaphore);
    }
}