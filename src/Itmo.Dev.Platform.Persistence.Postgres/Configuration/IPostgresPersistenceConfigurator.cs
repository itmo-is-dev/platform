using FluentMigrator.Runner.Initialization;
using Itmo.Dev.Platform.Options;
using Itmo.Dev.Platform.Persistence.Postgres.Conversions;
using Itmo.Dev.Platform.Persistence.Postgres.Models;
using Itmo.Dev.Platform.Persistence.Postgres.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Itmo.Dev.Platform.Persistence.Postgres.Configuration;

public interface IPostgresPersistenceConnectionConfigurator
{
    IPostgresPersistenceMigrationConfigurator WithConnectionOptions(
        Action<OptionsBuilder<PostgresConnectionOptions>> configuration);

    [ProducesOptionRegistration<PostgresConnectionOptions>(SectionParameterName = nameof(sectionPath))]
    IPostgresPersistenceMigrationConfigurator WithConnectionOptions(string sectionPath)
        => WithConnectionOptions(builder => builder.BindConfiguration(sectionPath));
}

public interface IPostgresPersistenceMigrationConfigurator
{
    IPostgresPersistenceConfigurator WithMigrationsFrom(params Assembly[] assemblies);

    IPostgresPersistenceConfigurator WithMigrationsFromItems(params IMigrationSourceItem[] items);
}

public interface IPostgresPersistenceConfigurator
{
    IPostgresPersistenceConfigurator WithDataSourcePlugin<T>()
        where T : class, IPostgresDataSourcePlugin;

    IPostgresPersistenceConfigurator WithStructConverter<TSource, TPrimitive>(
        IPlatformPostgresConverter<TSource, TPrimitive> converter)
        where TSource : struct;

    IPostgresPersistenceConfigurator WithConverter<TSource, TPrimitive>(
        IPlatformPostgresConverter<TSource, TPrimitive> converter)
        where TSource : class;
}
