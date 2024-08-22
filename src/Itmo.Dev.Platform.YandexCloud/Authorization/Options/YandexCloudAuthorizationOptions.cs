namespace Itmo.Dev.Platform.YandexCloud.Authorization.Options;

internal class YandexCloudAuthorizationOptions
{
    public YandexCloudVirtualMachineAuthorizationOptions VirtualMachine { get; set; } = new();
}