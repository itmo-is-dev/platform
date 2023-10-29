using Npgsql;
using System.Data;

namespace Itmo.Dev.Platform.Postgres.UnitOfWork;

public interface IUnitOfWork
{
    void Enqueue(NpgsqlCommand command);

    void Enqueue(NpgsqlCommand command, IUnitOfWorkResultHandler handler);

    ValueTask CommitAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken);
}