namespace Itmo.Dev.Platform.Locking.Redis;

public class RedisLockingOptions
{
    public string Endpoint { get; set; } = string.Empty;

    public string? KeyPrefix { get; set; }

    /// <summary>
    ///     Time after which Redis will automatically remove lock, if application crashed/did not remove it
    /// </summary>
    public TimeSpan ExpiryTime { get; set; }

    /// <summary>
    ///     Maximum time of waiting when lock for specified key is not available
    /// </summary>
    public TimeSpan WaitTime { get; set; }

    /// <summary>
    ///     Interval between retries of lock acquisition 
    /// </summary>
    public TimeSpan RetryInterval { get; set; }

    /// <summary>
    ///     Maximum value of added jitter to <see cref="RetryInterval"/>
    /// </summary>
    public int MaxRetryIntervalJitterMilliseconds { get; set; } = 100;
}
