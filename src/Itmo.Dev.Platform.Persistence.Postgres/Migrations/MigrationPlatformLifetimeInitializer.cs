using FluentMigrator.Runner;
using Itmo.Dev.Platform.Common.Lifetime.Initializers;
using Itmo.Dev.Platform.Persistence.Abstractions.Connections;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Persistence.Postgres.Migrations;

internal class MigrationPlatformLifetimeInitializer : PlatformLifetimeInitializerBase
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MigrationPlatformLifetimeInitializer(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();

        var connectionProvider = scope.ServiceProvider.GetRequiredService<IPersistenceConnectionProvider>();
        await using var connection = await connectionProvider.GetConnectionAsync(cancellationToken);

        await connection.ReloadTypesAsync(cancellationToken);
    }
}
