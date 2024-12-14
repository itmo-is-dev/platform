using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Itmo.Dev.Platform.Persistence.Postgres.Migrations;

internal class MigrationRunnerBackgroundService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MigrationRunnerBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

        runner.MigrateUp();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}