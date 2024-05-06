using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Itmo.Dev.Platform.Persistence.Postgres.Migrations;

internal class MigrationRunnerBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MigrationRunnerBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

        runner.MigrateUp();
    }
}