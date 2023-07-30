using Npgsql;

namespace Itmo.Dev.Platform.Postgres.Connection;

public interface IPostgresConnectionProvider
{
    ValueTask<NpgsqlConnection> GetConnectionAsync(CancellationToken cancellationToken);
}