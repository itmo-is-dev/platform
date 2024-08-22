namespace Itmo.Dev.Platform.YandexCloud.Lockbox.Options;

internal class YandexCloudLockboxOptions
{
    public bool IsEnabled { get; set; }

    public string SecretId { get; set; } = string.Empty;

    public TimeSpan LockboxOptionsPollingDelay { get; set; }
}