using Npgsql;
using System.Data;

namespace Itmo.Dev.Platform.Postgres.UnitOfWork;

public interface IUnitOfWork
{
    void Enqueue(NpgsqlCommand command);

    ValueTask CommitAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken);
}