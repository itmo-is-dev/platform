using FluentChaining;
using Itmo.Dev.Platform.YandexCloud.Configuration.Commands;
using Itmo.Dev.Platform.YandexCloud.Configuration.Links;
using Itmo.Dev.Platform.YandexCloud.Exceptions;
using Chain = FluentChaining.FluentChaining;

namespace Itmo.Dev.Platform.YandexCloud.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static async Task AddYandexCloudConfigurationAsync(this WebApplicationBuilder builder)
    {
        string deploymentEnv = builder.Configuration.GetValue<string>("Platform:Environment")
                               ?? throw new YandexCloudException("Deployment environment is not set.");

        IAsyncChain<ConfigurationCommand> chain = Chain.CreateAsyncChain<ConfigurationCommand>(
            start => start
                .Then<LocalConfigurationLink>()
                .Then<YandexCloudConfigurationLink>()
                .FinishWith(() => throw new YandexCloudException("Unknown deployment environment")));

        await chain.ProcessAsync(new ConfigurationCommand(deploymentEnv, builder));
    }
}