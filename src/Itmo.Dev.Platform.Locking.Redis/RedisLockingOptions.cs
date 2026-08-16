using Itmo.Dev.Platform.Options;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.Locking.Redis;

[OptionsType]
public class RedisLockingOptions
{
    [Required]
    public string Endpoint { get; set; } = string.Empty;

    public string? KeyPrefix { get; set; }

    /// <summary>
    ///     Time after which Redis will automatically remove lock, if application crashed/did not remove it
    /// </summary>
    [Required]
    [Description("Time after which Redis will automatically remove lock, if application crashed/did not remove it")]
    public TimeSpan ExpiryTime { get; set; }

    /// <summary>
    ///     Maximum time of waiting when lock for specified key is not available
    /// </summary>
    [Required]
    [Description("Maximum time of waiting when lock for specified key is not available")]
    public TimeSpan WaitTime { get; set; }

    /// <summary>
    ///     Interval between retries of lock acquisition 
    /// </summary>
    [Required]
    [Description("Interval between retries of lock acquisition ")]
    public TimeSpan RetryInterval { get; set; }

    /// <summary>
    ///     Maximum value of added jitter to <see cref="RetryInterval"/>
    /// </summary>
    [DefaultValue(100)]
    [Description("Maximum value of added jitter to RetryInterval")]
    public int MaxRetryIntervalJitterMilliseconds { get; set; } = 100;
}
