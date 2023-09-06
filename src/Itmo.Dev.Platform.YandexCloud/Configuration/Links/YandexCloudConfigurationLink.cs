using FluentChaining;
using Itmo.Dev.Platform.YandexCloud.Configuration.Commands;
using Itmo.Dev.Platform.YandexCloud.Exceptions;
using Itmo.Dev.Platform.YandexCloud.Models;
using Itmo.Dev.Platform.YandexCloud.Services;

namespace Itmo.Dev.Platform.YandexCloud.Configuration.Links;

internal class YandexCloudConfigurationLink : IAsyncLink<ConfigurationCommand>
{
    private const string EnvironmentName = "YandexCloud";

    public async Task<Unit> Process(
        ConfigurationCommand request,
        AsynchronousContext context,
        LinkDelegate<ConfigurationCommand, AsynchronousContext, Task<Unit>> next)
    {
        if (request.Environment.Equals(EnvironmentName, StringComparison.OrdinalIgnoreCase) is false)
        {
            return await next(request, context);
        }

        string secretId = request.ApplicationBuilder.Configuration.GetValue<string>("Platform:YandexCloud:LockBox:SecretId")
                          ?? throw new YandexCloudException("SecretId must be defined for Yandex Cloud deployment");

        var yandexCloudService = new YandexCloudService(request.ApplicationBuilder.Configuration);
        string token = await yandexCloudService.GetTokenAsync();

        var lockBoxClient = new YandexCloudLockBoxService(token);

        LockBoxEntry[] entries = await lockBoxClient.GetEntries(secretId);

        IConfigurationBuilder configurationBuilder = request.ApplicationBuilder.Configuration;
        configurationBuilder.Add(new LockBoxEntryConfigurationSource(entries));

        return Unit.Value;
    }
}