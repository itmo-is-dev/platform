using FluentMigrator.Runner.Initialization;
using Itmo.Dev.Platform.BackgroundTasks.Configuration;
using Itmo.Dev.Platform.BackgroundTasks.Persistence.Migrations;
using Itmo.Dev.Platform.Postgres.Extensions;
using Itmo.Dev.Platform.Postgres.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.BackgroundTasks.Extensions;

public static class ServiceScopeExtensions
{
    public static async Task UsePlatformBackgroundTasksAsync(this IServiceScope scope, CancellationToken cancellationToken)
    {
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