using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.MessagePersistence.Options;
using Itmo.Dev.Platform.MessagePersistence.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Configuration.Buffering;

internal class MessagePersistenceBufferingBuilder :
    IMessagePersistenceBufferingNameSelector,
    IMessagePersistenceBufferingPublisherConfigurationSelector,
    IMessagePersistenceBufferingStepSelector
{
    public MessagePersistenceBufferingBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection Services { get; }

    public string? BufferGroupName { get; private set; }

    public IMessagePersistenceBufferingPublisherConfigurationSelector Called(string name)
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

    public IMessagePersistenceBufferingStepSelector WithPublisherConfiguration(
        Action<OptionsBuilder<MessagePersistencePublisherOptions>> action)
    {
        var builder = Services.AddOptions<MessagePersistencePublisherOptions>(BufferGroupName);
        action.Invoke(builder);

        return this;
    }

    public IMessagePersistenceBufferingStepSelector WithStep(BufferStepOptions step)
    {
        Services.Configure<BufferGroupOptions>(BufferGroupName, options => options.Steps.Add(step));
        return this;
    }
}
