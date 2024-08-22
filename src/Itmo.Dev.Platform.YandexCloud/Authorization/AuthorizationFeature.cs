using Itmo.Dev.Platform.YandexCloud.Authorization.Options;
using Itmo.Dev.Platform.YandexCloud.Authorization.TokenProviders;

namespace Itmo.Dev.Platform.YandexCloud.Authorization;

internal static class AuthorizationFeature
{
    public static ValueTask<IYandexCloudTokenProvider> RegisterAsync(IHostApplicationBuilder builder)
    {
        var options = builder.Configuration
            .GetSection("Platform:YandexCloud:Authorization")
            .Get<YandexCloudAuthorizationOptions>();

        IYandexCloudTokenProvider provider = options switch
        {
            { VirtualMachine.IsEnabled: true } => new VirtualMachineTokenProvider(options.VirtualMachine),
            _ => new NullTokenProvider(),
        };

        builder.Services.AddSingleton(provider);

        return ValueTask.FromResult(provider);
    }
}