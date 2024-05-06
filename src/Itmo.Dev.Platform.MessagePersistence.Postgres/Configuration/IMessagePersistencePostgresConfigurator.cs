using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Postgres.Configuration;

public interface IMessagePersistencePostgresOptionsConfigurator
{
    IMessagePersistencePostgresConfigurator ConfigureOptions(
        Action<OptionsBuilder<MessagePersistencePostgresOptions>> action);
}

public interface IMessagePersistencePostgresConfigurator { }