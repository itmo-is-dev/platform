using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Postgres.Configuration;

public interface IMessagePersistencePostgresOptionsConfigurator
{
    IMessagePersistencePostgresConfigurator ConfigureOptions(
        Action<OptionsBuilder<MessagePersistencePostgresOptions>> action);

    IMessagePersistencePostgresConfigurator ConfigureOptions(string sectionPath)
    {
        return ConfigureOptions(builder => builder.BindConfiguration(sectionPath));
    }
}

public interface IMessagePersistencePostgresConfigurator { }