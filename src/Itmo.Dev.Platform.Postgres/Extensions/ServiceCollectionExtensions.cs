using FluentMigrator.Runner;
using Itmo.Dev.Platform.Postgres.Connection;
using Itmo.Dev.Platform.Postgres.Models;
using Itmo.Dev.Platform.Postgres.Plugins;
using Itmo.Dev.Platform.Postgres.Transactions;
using Itmo.Dev.Platform.Postgres.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Reflection;

namespace Itmo.Dev.Platform.Postgres.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformPostgres(
        this IServiceCollection collection,
        Action<OptionsBuilder<PostgresConnectionConfiguration>> configuration)
    {
        collection.AddSingleton<PostgresConnectionString>();

        collection.AddSingleton(p =>
        {
            var connectionString = p.GetRequiredService<PostgresConnectionString>();
            var loggerFactory = p.GetRequiredService<ILoggerFactory>();
            var plugins = p.GetRequiredService<IEnumerable<IDataSourcePlugin>>();

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString.Value);
            dataSourceBuilder.UseLoggerFactory(loggerFactory);

            foreach (IDataSourcePlugin plugin in plugins)
            {
                plugin.Configure(dataSourceBuilder);
            }

            return dataSourceBuilder.Build();
        });

        collection.AddSingleton<IPostgresConnectionFactory, DataSourceConnectionFactory>();

        collection.AddScoped<IPostgresConnectionProvider, PostgresConnectionProvider>();
        collection.AddScoped<IPostgresTransactionProvider, PostgresTransactionProvider>();
        collection.AddScoped<IUnitOfWork, ReusableUnitOfWork>();

        var optionsBuilder = collection.AddOptions<PostgresConnectionConfiguration>();
        configuration.Invoke(optionsBuilder);

        return collection;
    }

    public static IServiceCollection AddPlatformMigrations(
        this IServiceCollection collection,
        params Assembly[] assemblies)
    {
        return collection
            .AddFluentMigratorCore()
            .ConfigureRunner(builder => builder
                .AddPostgres()
                .WithMigrationsIn(assemblies)
                .WithGlobalConnectionString(provider => provider.GetRequiredService<PostgresConnectionString>().Value));
    }
}