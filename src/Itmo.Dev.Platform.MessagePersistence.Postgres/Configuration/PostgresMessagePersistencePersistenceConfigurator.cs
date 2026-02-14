using Itmo.Dev.Platform.Common.Lifetime.Extensions;
using Itmo.Dev.Platform.MessagePersistence.Internal.Persistence;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Migrations;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Plugins;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Queries;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Repositories;
using Itmo.Dev.Platform.Persistence.Postgres.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.MessagePersistence.Postgres.Configuration;

public class PostgresMessagePersistencePersistenceConfigurator : IMessagePersistencePersistenceConfigurator
{
    private readonly Action<IServiceCollection> _action;

    public PostgresMessagePersistencePersistenceConfigurator(Action<IServiceCollection> action)
    {
        _action = action;
    }

    public void Apply(IServiceCollection collection)
    {
        _action.Invoke(collection);

        collection.AddSingleton<MessagePersistenceQueryFactory>();
        collection.AddSingleton<MessagePersistenceQueryStorage>();

        collection.AddSingleton<IPostgresDataSourcePlugin, MappingPlugin>();
        collection.AddPlatformLifetimeInitializer<MessagePersistenceMigrationPlatformInitializer>();

        collection.AddScoped<MessagePersistenceRepository>();
        collection.AddScoped<IMessagePersistenceInternalRepository>(
            p => p.GetRequiredService<MessagePersistenceRepository>());
    }
}