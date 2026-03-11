using Itmo.Dev.Platform.Common.Serialization;
using Itmo.Dev.Platform.Persistence.Abstractions.Connections;
using Npgsql;

namespace Itmo.Dev.Platform.Persistence.Postgres.Connections;

internal class PostgresPersistenceConnectionProvider : IPersistenceConnectionProvider
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly IPlatformSerializer _serializer;

    public PostgresPersistenceConnectionProvider(NpgsqlDataSource dataSource, IPlatformSerializer serializer)
    {
        _dataSource = dataSource;
        _serializer = serializer;
    }

    public async ValueTask<IPersistenceConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        return new PostgresPersistenceConnection(connection, _serializer);
    }
}
