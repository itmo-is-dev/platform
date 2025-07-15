using Itmo.Dev.Platform.MessagePersistence.Configuration.General;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Configuration;

namespace Itmo.Dev.Platform.MessagePersistence.Postgres.Extensions;

public static class ServiceCollectionExtensions
{
    public static IMessagePersistenceBufferingGroupConfigurator UsePostgresPersistence(
        this IMessagePersistencePersistenceConfigurationSelector selector,
        Func<IMessagePersistencePostgresOptionsConfigurator, IMessagePersistencePostgresConfigurator> configuration)
    {
        var configurator = new PostgresMessagePersistencePersistenceConfigurator(
            collection =>
            {
                var postgresConfigurator = new MessagePersistencePostgresConfigurator(collection);
                configuration.Invoke(postgresConfigurator);
            });

        return selector.UsePersistenceConfigurator(configurator);
    }
}