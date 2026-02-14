using Itmo.Dev.Platform.MessagePersistence.Postgres.Configuration;

namespace Itmo.Dev.Platform.MessagePersistence.Postgres.Extensions;

public static class ServiceCollectionExtensions
{
    public static Config.General.IBufferingGroupStep UsePostgresPersistence(
        this Config.General.IPersistenceStep selector,
        Func<PostgresConfig.IOptionsStep, PostgresConfig.IFinalStep> configuration)
    {
        var configurator = new PostgresMessagePersistencePersistenceConfigurator(collection =>
        {
            var postgresConfigurator = new MessagePersistencePostgresConfigurator(collection);
            configuration.Invoke(postgresConfigurator);
        });

        return selector.UsePersistenceConfigurator(configurator);
    }
}
