using FluentMigrator.Runner.Initialization;
using Itmo.Dev.Platform.MessagePersistence.Configuration;
using Itmo.Dev.Platform.Postgres.Extensions;
using Itmo.Dev.Platform.Postgres.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Persistence.Migrations;

internal class MigrationRunnerService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MigrationRunnerService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var outerScope = _scopeFactory.CreateAsyncScope();
        
        var collection = new ServiceCollection();

        var connectionString = outerScope.ServiceProvider.GetRequiredService<PostgresConnectionString>();
        collection.AddSingleton(connectionString);

        var options = outerScope.ServiceProvider.GetRequiredService<IOptions<MessagePersistencePersistenceOptions>>();
        collection.AddSingleton(options);

        collection.AddSingleton<IMigrationSourceItem>(new MigrationSourceItem());
        collection.AddPlatformMigrations();

        var provider = collection.BuildServiceProvider();
        await using var innerScope = provider.CreateAsyncScope();

        await innerScope.UsePlatformMigrationsAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}