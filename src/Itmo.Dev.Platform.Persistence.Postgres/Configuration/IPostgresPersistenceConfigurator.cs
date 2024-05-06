using FluentMigrator.Runner.Initialization;
using Itmo.Dev.Platform.Persistence.Postgres.Models;
using Itmo.Dev.Platform.Persistence.Postgres.Plugins;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Itmo.Dev.Platform.Persistence.Postgres.Configuration;

public interface IPostgresPersistenceConnectionConfigurator
{
    IPostgresPersistenceMigrationConfigurator WithConnectionOptions(
        Action<OptionsBuilder<PostgresConnectionOptions>> configuration);
}

public interface IPostgresPersistenceMigrationConfigurator
{
    IPostgresPersistencePluginConfigurator WithMigrationsFrom(params Assembly[] assemblies);

    IPostgresPersistencePluginConfigurator WithMigrationsFromItems(params IMigrationSourceItem[] items);
}

public interface IPostgresPersistencePluginConfigurator : IPostgresPersistenceConfigurator
{
    IPostgresPersistencePluginConfigurator WithDataSourcePlugin<T>()
        where T : class, IPostgresDataSourcePlugin;
}

public interface IPostgresPersistenceConfigurator { }