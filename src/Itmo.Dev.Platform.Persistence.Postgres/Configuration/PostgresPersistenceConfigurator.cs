using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Itmo.Dev.Platform.Persistence.Postgres.Connections;
using Itmo.Dev.Platform.Persistence.Postgres.Conversions;
using Itmo.Dev.Platform.Persistence.Postgres.Conversions.Initializers;
using Itmo.Dev.Platform.Persistence.Postgres.Models;
using Itmo.Dev.Platform.Persistence.Postgres.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Itmo.Dev.Platform.Persistence.Postgres.Configuration;

internal class PostgresPersistenceConfigurator :
    IPostgresPersistenceConnectionConfigurator,
    IPostgresPersistenceMigrationConfigurator,
    IPostgresPersistenceConfigurator
{
    private readonly IServiceCollection _collection;

    public PostgresPersistenceConfigurator(IServiceCollection collection)
    {
        _collection = collection;
    }

    public IPostgresPersistenceMigrationConfigurator WithConnectionOptions(
        Action<OptionsBuilder<PostgresConnectionOptions>> configuration)
    {
        var builder = _collection
            .AddOptions<PostgresConnectionOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        configuration.Invoke(builder);

        return this;
    }

    public IPostgresPersistenceConfigurator WithMigrationsFrom(params Assembly[] assemblies)
    {
        if (assemblies is [])
            return this;

        _collection
            .AddFluentMigratorCore()
            .ConfigureRunner(builder => builder
                .AddPostgres()
                .WithMigrationsIn(assemblies)
                .WithGlobalConnectionString(provider => provider
                    .GetRequiredService<IPostgresConnectionStringProvider>()
                    .GetConnectionString()));

        return this;
    }

    public IPostgresPersistenceConfigurator WithMigrationsFromItems(params IMigrationSourceItem[] items)
    {
        if (items is [])
            return this;

        _collection
            .AddFluentMigratorCore()
            .ConfigureRunner(builder => builder
                .AddPostgres()
                .WithGlobalConnectionString(provider => provider
                    .GetRequiredService<IPostgresConnectionStringProvider>()
                    .GetConnectionString()));

        foreach (IMigrationSourceItem item in items)
        {
            _collection.TryAddEnumerable(ServiceDescriptor.Singleton(item));
        }

        return this;
    }

    public IPostgresPersistenceConfigurator WithDataSourcePlugin<T>()
        where T : class, IPostgresDataSourcePlugin
    {
        _collection.TryAddEnumerable(ServiceDescriptor.Singleton<IPostgresDataSourcePlugin, T>());
        return this;
    }

    public IPostgresPersistenceConfigurator WithStructConverter<TSource, TPrimitive>(
        IPlatformPostgresConverter<TSource, TPrimitive> converter)
        where TSource : struct
    {
        _collection.TryAddEnumerable(ServiceDescriptor.Singleton<IPlatformConverterInitializer>(
            new StructConversionInitializer<TSource, TPrimitive>(converter)));

        return this;
    }

    public IPostgresPersistenceConfigurator WithConverter<TSource, TPrimitive>(
        IPlatformPostgresConverter<TSource, TPrimitive> converter)
        where TSource : class
    {
        _collection.TryAddEnumerable(ServiceDescriptor.Singleton<IPlatformConverterInitializer>(
            new ClassConversionInitializer<TSource, TPrimitive>(converter)));

        return this;
    }
}
