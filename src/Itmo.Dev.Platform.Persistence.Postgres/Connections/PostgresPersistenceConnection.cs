using Itmo.Dev.Platform.Persistence.Abstractions.Commands;
using Itmo.Dev.Platform.Persistence.Abstractions.Connections;
using Itmo.Dev.Platform.Persistence.Postgres.Commands;
using Npgsql;

namespace Itmo.Dev.Platform.Persistence.Postgres.Connections;

internal class PostgresPersistenceConnection : IPersistenceConnection
{
    public PostgresPersistenceConnection(NpgsqlConnection connection)
    {
        Connection = connection;
    }

    public NpgsqlConnection Connection { get; }

    public IPersistenceCommand CreateCommand(string query)
    {
#pragma warning disable CA2100
        return new PostgresPersistenceCommand(new NpgsqlCommand(query, Connection));
#pragma warning restore CA2100
    }

    public async ValueTask DisposeAsync()
    {
        await Connection.DisposeAsync();
    }
}