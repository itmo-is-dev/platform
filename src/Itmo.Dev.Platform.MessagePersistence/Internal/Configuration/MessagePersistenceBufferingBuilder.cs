using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.MessagePersistence.Internal.Options;
using Itmo.Dev.Platform.MessagePersistence.Internal.Services;
using Itmo.Dev.Platform.MessagePersistence.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Configuration;

internal class MessagePersistenceBufferingBuilder :
    Config.Buffering.INameStep,
    Config.Buffering.IConfigurationStep,
    Config.Buffering.IBufferingStepStep
{
    public MessagePersistenceBufferingBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection Services { get; }

    public string BufferGroupName { get; private set; } = string.Empty;

    public Config.Buffering.IConfigurationStep Called(string name)
    {
        BufferGroupName = name;

        Services.AddHostedServiceUnsafe(provider
            => ActivatorUtilities.CreateInstance<MessagePersistenceInitialPublishBackgroundService>(
                provider,
                name));

        Services
            .AddOptions<MessagePersistencePublisherOptions>(name)
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .Configure(options => options.IsInitialized = true);

        Services
            .AddOptions<BufferGroupOptions>(name)
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .Configure(options =>
            {
                options.IsInitialized = true;
                options.Name = name;
            });

        return this;
    }

    public Config.Buffering.IBufferingStepStep WithPublisherConfiguration(
        Action<OptionsBuilder<MessagePersistencePublisherOptions>> action)
    {
        var builder = Services.AddOptions<MessagePersistencePublisherOptions>(BufferGroupName);
        action.Invoke(builder);

        return this;
    }

    public Config.Buffering.IBufferingStepStep WithStep(BufferStepOptions step)
    {
        Services.Configure<BufferGroupOptions>(BufferGroupName, options => options.Steps.Add(step));
        return this;
    }
}
