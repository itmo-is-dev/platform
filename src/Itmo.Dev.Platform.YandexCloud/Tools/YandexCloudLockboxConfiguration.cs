namespace Itmo.Dev.Platform.YandexCloud.Tools;

public class YandexCloudLockboxConfiguration
{
    public string SecretId { get; set; } = string.Empty;
    
    public int LockboxOptionsPollingDelaySeconds { get; set; }

    public TimeSpan LockboxOptionsPollingDelay => TimeSpan.FromSeconds(LockboxOptionsPollingDelaySeconds);
}