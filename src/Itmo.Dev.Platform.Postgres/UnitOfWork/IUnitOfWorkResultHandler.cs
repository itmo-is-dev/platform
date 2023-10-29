using Npgsql;

namespace Itmo.Dev.Platform.Postgres.UnitOfWork;

public interface IUnitOfWorkResultHandler
{
    Task HandleReaderAsync(NpgsqlDataReader reader, CancellationToken cancellationToken);
}