using FluentMigrator.Runner;
using Itmo.Dev.Platform.Postgres.Connection;
using Itmo.Dev.Platform.Postgres.Models;
using Itmo.Dev.Platform.Postgres.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Itmo.Dev.Platform.Postgres.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformPostgres(
        this IServiceCollection collection,
        Action<OptionsBuilder<PostgresConnectionConfiguration>> configuration)
    {
        collection.AddSingleton<PostgresConnectionString>();

        collection.AddScoped<IPostgresConnectionProvider, PostgresConnectionProvider>();
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