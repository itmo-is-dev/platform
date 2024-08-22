using Itmo.Dev.Platform.YandexCloud.Authorization;
using Itmo.Dev.Platform.YandexCloud.Lockbox;

namespace Itmo.Dev.Platform.YandexCloud;

public static class WebApplicationBuilderExtensions
{
    public static async ValueTask AddPlatformYandexCloudAsync(this IHostApplicationBuilder builder)
    {
        var tokenProvider = await AuthorizationFeature.RegisterAsync(builder);
        await LockboxFeature.RegisterAsync(builder, tokenProvider);
    }
}