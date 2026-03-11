using Itmo.Dev.Platform.Common.Serialization;
using Itmo.Dev.Platform.Persistence.Abstractions.Commands;
using Itmo.Dev.Platform.Persistence.Abstractions.Connections;
using Itmo.Dev.Platform.Persistence.Postgres.Commands;
using Npgsql;

namespace Itmo.Dev.Platform.Persistence.Postgres.Connections;

internal class PostgresPersistenceConnection : IPersistenceConnection
{
    private readonly IPlatformSerializer _serializer;

    public PostgresPersistenceConnection(NpgsqlConnection connection, IPlatformSerializer serializer)
    {
        Connection = connection;
        _serializer = serializer;
    }

    public NpgsqlConnection Connection { get; }

    public IPersistenceCommand CreateCommand(string query)
    {
#pragma warning disable CA2100
        return new PostgresPersistenceCommand(new NpgsqlCommand(query, Connection), _serializer);
#pragma warning restore CA2100
    }

    public async ValueTask DisposeAsync()
    {
        await Connection.DisposeAsync();
    }
}
