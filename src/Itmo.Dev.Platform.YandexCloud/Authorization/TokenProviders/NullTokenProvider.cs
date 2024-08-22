using Itmo.Dev.Platform.YandexCloud.Exceptions;

namespace Itmo.Dev.Platform.YandexCloud.Authorization.TokenProviders;

public class NullTokenProvider : IYandexCloudTokenProvider
{
    public Task<string> GetTokenAsync(CancellationToken cancellationToken)
        => throw new YandexCloudException("Please specify correct Platform:YandexCloud:Authorization configuration");
}