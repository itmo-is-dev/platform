using FluentMigrator.Runner;
using Itmo.Dev.Platform.BackgroundTasks.Postgres.Configuration;
using Itmo.Dev.Platform.BackgroundTasks.Postgres.Plugins;
using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.Common.Lifetime.Initializers;
using Itmo.Dev.Platform.Persistence.Abstractions.Extensions;
using Itmo.Dev.Platform.Persistence.Postgres.Extensions;
using Itmo.Dev.Platform.Persistence.Postgres.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.BackgroundTasks.Postgres.Migrations;

public class BackgroundTasksMigrationPlatformInitializer : PlatformLifetimeInitializerBase
{
    private readonly IServiceScopeFactory _scopeFactory;

    public BackgroundTasksMigrationPlatformInitializer(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var collection = new ServiceCollection();

        var connectionOptions = scope.ServiceProvider.GetRequiredService<IOptions<PostgresConnectionOptions>>();
        var persistenceOptions = scope.ServiceProvider.GetRequiredService<IOptions<BackgroundTaskPersistenceOptions>>();

        collection
            .AddOptions<BackgroundTaskPersistenceOptions>()
            .Configure(o => persistenceOptions.Value.ApplyTo(o));

        collection.AddPlatform();

        collection.AddPlatformPersistence(
            persistence => persistence.UsePostgres(
                postgres => postgres
                    .WithConnectionOptions(builder => builder.Configure(o => connectionOptions.Value.ApplyTo(o)))
                    .WithMigrationsFromItems(new MigrationSourceItem())
                    .WithDataSourcePlugin<BackgroundTaskDataSourcePlugin>()));

        var provider = collection.BuildServiceProvider();
        await using var innerScope = provider.CreateAsyncScope();

        var runner = innerScope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }
}