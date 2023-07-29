using System.Data.Common;

namespace Itmo.Dev.Platform.Postgres.Connection;

public interface IPostgresConnectionProvider
{
    ValueTask<DbConnection> GetConnectionAsync(CancellationToken cancellationToken);
}