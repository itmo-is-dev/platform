using Itmo.Dev.Platform.Locking.Exceptions;
using Microsoft.Extensions.Options;
using RedLockNet;
using RedLockNet.SERedis;

namespace Itmo.Dev.Platform.Locking.Redis;

internal class RedisLockService : ILockingService
{
    private readonly RedLockFactory _redLockFactory;
    private readonly IOptionsMonitor<RedisLockingOptions> _options;
    private readonly ILockingKeyFormatter _lockingKeyFormatter;

    public RedisLockService(
        RedLockFactory redLockFactory,
        IOptionsMonitor<RedisLockingOptions> options,
        ILockingKeyFormatter lockingKeyFormatter)
    {
        _redLockFactory = redLockFactory;
        _options = options;
        _lockingKeyFormatter = lockingKeyFormatter;
    }

    public async ValueTask<ILockHandle> AcquireAsync(object key, CancellationToken cancellationToken)
    {
        var options = _options.CurrentValue;
        IRedLock lck;

        try
        {
            lck = await _redLockFactory.CreateLockAsync(
                resource: _lockingKeyFormatter.Format(key),
                expiryTime: options.ExpiryTime,
                waitTime: options.WaitTime,
                retryTime: options.RetryInterval,
                cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            throw PlatformLockingException.Error(e);
        }

        if (lck.IsAcquired is false)
        {
            throw PlatformLockingException.FailedToAcquire();
        }

        return new RedisLockHandle(lck);
    }
}
