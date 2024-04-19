using FluentMigrator.Runner.Initialization;
using Itmo.Dev.Platform.BackgroundTasks.Configuration;
using Itmo.Dev.Platform.Common.Lifetime.Initializers;
using Itmo.Dev.Platform.Common.Lifetime.Services;
using Itmo.Dev.Platform.Postgres.Extensions;
using Itmo.Dev.Platform.Postgres.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.BackgroundTasks.Persistence.Migrations;

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

        var connectionString = scope.ServiceProvider.GetRequiredService<PostgresConnectionString>();
        collection.AddSingleton(connectionString);

        var options = scope.ServiceProvider.GetRequiredService<IOptions<BackgroundTaskPersistenceOptions>>();
        collection.AddSingleton(options);

        collection.AddSingleton<IMigrationSourceItem>(new MigrationSourceItem());
        collection.AddPlatformMigrations();

        var provider = collection.BuildServiceProvider();
        await using var innerScope = provider.CreateAsyncScope();

        await innerScope.UsePlatformMigrationsAsync(cancellationToken);
    }
}