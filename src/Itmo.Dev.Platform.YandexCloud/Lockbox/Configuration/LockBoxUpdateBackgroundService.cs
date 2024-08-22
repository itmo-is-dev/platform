using Itmo.Dev.Platform.Common.BackgroundServices;
using Itmo.Dev.Platform.YandexCloud.Lockbox.Options;
using Itmo.Dev.Platform.YandexCloud.Lockbox.Services;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.YandexCloud.Lockbox.Configuration;

internal class LockBoxUpdateBackgroundService : RestartableBackgroundService
{
    private readonly IOptionsMonitor<YandexCloudLockboxOptions> _options;
    private readonly YandexCloudLockBoxService _lockBoxService;
    private readonly LockBoxEntryConfigurationProvider _configurationProvider;

    public LockBoxUpdateBackgroundService(
        IOptionsMonitor<YandexCloudLockboxOptions> options,
        YandexCloudLockBoxService lockBoxService,
        LockBoxEntryConfigurationProvider configurationProvider)
    {
        _options = options;
        _lockBoxService = lockBoxService;
        _configurationProvider = configurationProvider;
    }

    protected override async Task ExecuteAsync(CancellationTokenSource cts)
    {
        using var _ = _options.OnChange(_ => cts.Cancel());
        var configuration = _options.CurrentValue;

        while (cts.IsCancellationRequested is false)
        {
            await Task.Delay(configuration.LockboxOptionsPollingDelay, cts.Token);

            var entries = await _lockBoxService.GetEntries(configuration.SecretId, cts.Token);
            _configurationProvider.UpdateValues(entries);
        }
    }
}