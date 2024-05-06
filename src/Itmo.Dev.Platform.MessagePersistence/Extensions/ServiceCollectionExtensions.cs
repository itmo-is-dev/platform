using Itmo.Dev.Platform.MessagePersistence.Configuration.Builders;
using Itmo.Dev.Platform.MessagePersistence.Configuration.General;
using Itmo.Dev.Platform.MessagePersistence.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.MessagePersistence.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformMessagePersistence(
        this IServiceCollection collection,
        Func<IMessagePersistencePersistenceConfigurationSelector, IMessagePersistenceConfigurationBuilder> config)
    {
        var builder = new MessagePersistenceConfigurationBuilder(collection);
        config.Invoke(builder);

        collection.AddScoped<IMessagePersistenceConsumer, MessagePersistenceConsumer>();

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