using Itmo.Dev.Platform.Locking.Exceptions;
using Microsoft.Extensions.Options;
using RedLockNet;
using RedLockNet.SERedis;

namespace Itmo.Dev.Platform.Locking.Redis;

internal class RedisLockService : ILockingService
{
    private readonly RedLockFactory _redLockFactory;
    private readonly IOptionsMonitor<RedisLockingOptions> _options;
    private readonly IKeyFormattingStrategy _keyFormattingStrategy;

    public RedisLockService(
        RedLockFactory redLockFactory,
        IOptionsMonitor<RedisLockingOptions> options,
        IKeyFormattingStrategy keyFormattingStrategy)
    {
        _redLockFactory = redLockFactory;
        _options = options;
        _keyFormattingStrategy = keyFormattingStrategy;
    }

    public async ValueTask<ILockHandle> AcquireAsync(object key, CancellationToken cancellationToken)
    {
        var options = _options.CurrentValue;
        IRedLock lck;

        try
        {
            lck = await _redLockFactory.CreateLockAsync(
                resource: _keyFormattingStrategy.Format(key),
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
