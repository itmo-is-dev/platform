using RedLockNet;

namespace Itmo.Dev.Platform.Locking.Redis;

internal class RedisLockHandle : ILockHandle
{
    private readonly IRedLock _lock;

    public RedisLockHandle(IRedLock @lock)
    {
        _lock = @lock;
    }

    public async ValueTask DisposeAsync()
    {
        await _lock.DisposeAsync();
    }
}
