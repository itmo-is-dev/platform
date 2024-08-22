namespace Itmo.Dev.Platform.YandexCloud.Authorization.Options;

internal class YandexCloudVirtualMachineAuthorizationOptions
{
    public bool IsEnabled { get; set; }

    public Uri? ServiceUri { get; set; }

    public TimeSpan MinRemainingTokenLifetime { get; set; }
}