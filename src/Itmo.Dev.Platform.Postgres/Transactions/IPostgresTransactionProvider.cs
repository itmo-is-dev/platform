using System.Data;

namespace Itmo.Dev.Platform.Postgres.Transactions;

public interface IPostgresTransactionProvider
{
    Task<IPostgresTransaction> CreateTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken);
}