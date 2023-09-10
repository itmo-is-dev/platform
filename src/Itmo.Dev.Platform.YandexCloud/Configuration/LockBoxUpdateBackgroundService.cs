using Itmo.Dev.Platform.Common.BackgroundServices;
using Itmo.Dev.Platform.YandexCloud.Services;
using Itmo.Dev.Platform.YandexCloud.Tools;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.YandexCloud.Configuration;

internal class LockBoxUpdateBackgroundService : RestartableBackgroundService
{
    private readonly IOptionsMonitor<YandexCloudLockboxConfiguration> _options;
    private readonly YandexCloudLockBoxService _lockBoxService;
    private readonly LockBoxEntryConfigurationProvider _configurationProvider;

    public LockBoxUpdateBackgroundService(
        IOptionsMonitor<YandexCloudLockboxConfiguration> options,
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