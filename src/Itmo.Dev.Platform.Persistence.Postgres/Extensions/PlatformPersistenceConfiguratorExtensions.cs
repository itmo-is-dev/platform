using Itmo.Dev.Platform.Common.Lifetime.Extensions;
using Itmo.Dev.Platform.Persistence.Abstractions.Configuration;
using Itmo.Dev.Platform.Persistence.Postgres.Configuration;
using Itmo.Dev.Platform.Persistence.Postgres.Connections;
using Itmo.Dev.Platform.Persistence.Postgres.Migrations;
using Itmo.Dev.Platform.Persistence.Postgres.Plugins;
using Itmo.Dev.Platform.Persistence.Postgres.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Itmo.Dev.Platform.Persistence.Postgres.Extensions;

public static class PlatformPersistenceConfiguratorExtensions
{
    public static IPlatformPersistenceConfigurator UsePostgres(
        this IPlatformPersistenceConnectionProviderConfigurator connectionProviderConfigurator,
        Func<IPostgresPersistenceConnectionConfigurator, IPostgresPersistenceConfigurator> postgresConfiguration)
    {
        var configurator = connectionProviderConfigurator
            .UseConnectionProvider<PostgresPersistenceConnectionProvider>()
            .UseTransactionProvider<PostgresPersistenceTransactionProvider>();

        var postgresConfigurator = new PostgresPersistenceConfigurator(configurator.Services);
        postgresConfiguration.Invoke(postgresConfigurator);

        configurator.Services.AddPlatformLifetimePostInitializer<MigrationPlatformLifetimePostInitializer>();

        configurator.Services.AddSingleton<IPostgresConnectionStringProvider, PostgresConnectionStringProvider>();

        configurator.Services.AddSingleton(
            provider =>
            {
                var connectionStringProvider = provider.GetRequiredService<IPostgresConnectionStringProvider>();
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var plugins = provider.GetRequiredService<IEnumerable<IPostgresDataSourcePlugin>>();

                var builder = new NpgsqlDataSourceBuilder(connectionStringProvider.GetConnectionString());
                builder.UseLoggerFactory(loggerFactory);

                foreach (IPostgresDataSourcePlugin plugin in plugins)
                {
                    plugin.Configure(builder);
                }

                return builder.Build();
            });

        return configurator;
    }
}