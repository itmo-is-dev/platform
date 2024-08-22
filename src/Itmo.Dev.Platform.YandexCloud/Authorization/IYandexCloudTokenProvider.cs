namespace Itmo.Dev.Platform.YandexCloud.Authorization;

internal interface IYandexCloudTokenProvider
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken);
}