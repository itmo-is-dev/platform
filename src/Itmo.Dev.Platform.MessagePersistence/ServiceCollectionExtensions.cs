using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.MessagePersistence.Configuration.General;
using Itmo.Dev.Platform.MessagePersistence.Configuration.MessageHandlers;
using Itmo.Dev.Platform.MessagePersistence.Execution;
using Itmo.Dev.Platform.MessagePersistence.Options;
using Itmo.Dev.Platform.MessagePersistence.Options.Validators;
using Itmo.Dev.Platform.MessagePersistence.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformMessagePersistence(
        this IServiceCollection collection,
        Func<IMessagePersistenceDefaultPublisherConfigurationSelector, IMessagePersistenceConfigurationBuilder> config)
    {
        collection.AddSingleton<MessagePersistenceRegistry>();
        collection.AddScoped<IMessagePersistenceConsumer, MessagePersistenceConsumer>();
        collection.AddScoped<IMessagePersistencePublisher, MessagePersistencePublisher>();

        collection.AddScoped<IMessagePersistenceExecutor, MessagePersistenceExecutor>();
        collection.AddScoped<IMessagePersistenceBufferingExecutor, MessagePersistenceBufferingExecutor>();

        collection.AddSingleton<IValidateOptions<PersistedMessageOptions>, PersistedMessageBufferGroupValidator>();
        collection.AddSingleton<IValidateOptions<BufferGroupOptions>, BufferingGroupPublisherValidator>();

        collection.AddHostedServiceUnsafe(provider
            => ActivatorUtilities.CreateInstance<MessagePersistenceInitialPublishBackgroundService>(
                provider,
                MessagePersistenceConstants.DefaultPublisherName));

        var builder = new MessagePersistenceConfigurationBuilder(collection);
        config.Invoke(builder);

        return collection;
    }

    internal static IServiceCollection AddPlatformMessagePersistenceHandler(
        this IServiceCollection collection,
        Func<IMessagePersistenceHandlerNameConfigurator, IMessagePersistenceHandlerBuilder> configuration)
    {
        var configurator = new MessagePersistenceHandlerNameConfigurator(collection);
        configuration.Invoke(configurator).Build();

        return collection;
    }
}
