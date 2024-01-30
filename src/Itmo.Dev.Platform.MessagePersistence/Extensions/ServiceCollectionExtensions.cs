using Itmo.Dev.Platform.MessagePersistence.Configuration.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.MessagePersistence.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformMessagePersistence(
        this IServiceCollection collection,
        Func<IMessagePersistencePersistenceConfigurator, IMessagePersistenceConfigurationBuilder> configuration)
    {
        var builder = new MessagePersistenceConfigurationBuilder(collection);
        configuration.Invoke(builder);

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