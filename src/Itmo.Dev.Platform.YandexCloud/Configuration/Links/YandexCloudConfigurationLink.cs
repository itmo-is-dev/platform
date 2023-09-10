using FluentChaining;
using Itmo.Dev.Platform.YandexCloud.Configuration.Commands;
using Itmo.Dev.Platform.YandexCloud.Exceptions;
using Itmo.Dev.Platform.YandexCloud.Models;
using Itmo.Dev.Platform.YandexCloud.Services;
using Itmo.Dev.Platform.YandexCloud.Tools;

namespace Itmo.Dev.Platform.YandexCloud.Configuration.Links;

internal class YandexCloudConfigurationLink : IAsyncLink<ConfigurationCommand>
{
    private const string EnvironmentName = "YandexCloud";
    private const string SecretIdKey = "Platform:YandexCloud:LockBox:SecretId";

    public async Task<Unit> Process(
        ConfigurationCommand request,
        AsynchronousContext context,
        LinkDelegate<ConfigurationCommand, AsynchronousContext, Task<Unit>> next)
    {
        if (request.Environment.Equals(EnvironmentName, StringComparison.OrdinalIgnoreCase) is false)
        {
            return await next(request, context);
        }

        string? secretId = request.ApplicationBuilder.Configuration.GetValue<string>(SecretIdKey);

        if (secretId is null)
            throw new YandexCloudException("SecretId must be defined for Yandex Cloud deployment");

        request.ApplicationBuilder.Services
            .AddOptions<YandexCloudLockboxConfiguration>()
            .BindConfiguration("Platform:YandexCloud:LockBox");

        var tokenProvider = new YandexCloudTokenProvider(request.ApplicationBuilder.Configuration);
        var lockBoxService = new YandexCloudLockBoxService(tokenProvider);

        request.ApplicationBuilder.Services.AddSingleton(tokenProvider);
        request.ApplicationBuilder.Services.AddSingleton(lockBoxService);

        LockBoxEntry[] entries = await lockBoxService.GetEntries(secretId, context.CancellationToken);

        var provider = new LockBoxEntryConfigurationProvider(entries);
        request.ApplicationBuilder.Services.AddSingleton(provider);

        IConfigurationBuilder configurationBuilder = request.ApplicationBuilder.Configuration;
        configurationBuilder.Add(new ConfigurationSource(provider));

        return Unit.Value;
    }
}