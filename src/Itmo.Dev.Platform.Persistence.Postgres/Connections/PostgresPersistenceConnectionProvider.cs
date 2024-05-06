using Itmo.Dev.Platform.Persistence.Abstractions.Connections;
using Npgsql;

namespace Itmo.Dev.Platform.Persistence.Postgres.Connections;

internal class PostgresPersistenceConnectionProvider : IPersistenceConnectionProvider
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgresPersistenceConnectionProvider(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async ValueTask<IPersistenceConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        return new PostgresPersistenceConnection(connection);
    }
}