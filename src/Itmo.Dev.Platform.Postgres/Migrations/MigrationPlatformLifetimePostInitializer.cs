using Itmo.Dev.Platform.Common.Lifetime.Initializers;
using Itmo.Dev.Platform.Postgres.Connection;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Postgres.Migrations;

internal class MigrationPlatformLifetimePostInitializer : PlatformLifetimePostInitializerBase
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MigrationPlatformLifetimePostInitializer(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var connectionProvider = scope.ServiceProvider.GetRequiredService<IPostgresConnectionProvider>();
        var connection = await connectionProvider.GetConnectionAsync(cancellationToken);

        await connection.ReloadTypesAsync();
    }
}