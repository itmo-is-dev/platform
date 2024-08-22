using Itmo.Dev.Platform.YandexCloud.Authorization;
using Itmo.Dev.Platform.YandexCloud.Exceptions;
using Itmo.Dev.Platform.YandexCloud.Lockbox.Configuration;
using Itmo.Dev.Platform.YandexCloud.Lockbox.Models;
using Itmo.Dev.Platform.YandexCloud.Lockbox.Options;
using Itmo.Dev.Platform.YandexCloud.Lockbox.Services;

namespace Itmo.Dev.Platform.YandexCloud.Lockbox;

internal static class LockboxFeature
{
    public static async ValueTask RegisterAsync(IHostApplicationBuilder builder, IYandexCloudTokenProvider tokenProvider)
    {
        var options = builder.Configuration
            .GetSection("Platform:YandexCloud:Lockbox")
            .Get<YandexCloudLockboxOptions>();

        if (options is null)
            return;

        if (options.IsEnabled is false)
            return;

        if (options.SecretId is null)
        {
            throw new YandexCloudException(
                "Platform:YandexCloud:LockBox:SecretId must be defined when Lockbox is enabled");
        }

        var lockBoxService = new YandexCloudLockBoxService(tokenProvider);

        builder.Services.AddSingleton(lockBoxService);
        builder.Services.AddHostedService<LockBoxUpdateBackgroundService>();

        LockBoxEntry[] entries = await lockBoxService.GetEntries(options.SecretId, default);

        var provider = new LockBoxEntryConfigurationProvider(entries);
        builder.Services.AddSingleton(provider);

        IConfigurationBuilder configurationBuilder = builder.Configuration;
        configurationBuilder.Add(new ConfigurationSource(provider));
    }
}