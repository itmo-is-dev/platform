using Itmo.Dev.Platform.Common.Lifetime.Initializers;
using Itmo.Dev.Platform.Persistence.Abstractions.Connections;
using Itmo.Dev.Platform.Persistence.Postgres.Connections;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Persistence.Postgres.Migrations;

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

        var connectionProvider = scope.ServiceProvider.GetRequiredService<IPersistenceConnectionProvider>();
        await using var connection = await connectionProvider.GetConnectionAsync(cancellationToken);

        if (connection is not PostgresPersistenceConnection postgresConnection)
        {
            throw new InvalidOperationException(
                "Trying to apply migration Postgres initializer on non-postgres persistence configuration");
        }

        await postgresConnection.Connection.ReloadTypesAsync();
    }
}