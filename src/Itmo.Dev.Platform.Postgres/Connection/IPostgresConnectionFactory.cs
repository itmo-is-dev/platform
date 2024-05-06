using Npgsql;

namespace Itmo.Dev.Platform.Postgres.Connection;

internal interface IPostgresConnectionFactory
{
    NpgsqlConnection CreateConnection();

    ValueTask<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken);
}