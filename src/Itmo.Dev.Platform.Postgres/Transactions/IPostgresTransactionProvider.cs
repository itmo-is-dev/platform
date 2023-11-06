using Npgsql;
using System.Data;

namespace Itmo.Dev.Platform.Postgres.Transactions;

public interface IPostgresTransactionProvider
{
    Task<NpgsqlTransaction> CreateTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken);
}