using IsolationLevel = System.Data.IsolationLevel;

namespace Itmo.Dev.Platform.Postgres.Transactions;

public class PostgresTransactionProvider : IPostgresTransactionProvider
{
    public Task<IPostgresTransaction> CreateTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken)
    {
        System.Transactions.IsolationLevel level = isolationLevel switch
        {
            IsolationLevel.Unspecified => System.Transactions.IsolationLevel.Unspecified,
            IsolationLevel.Chaos => System.Transactions.IsolationLevel.Chaos,
            IsolationLevel.ReadUncommitted => System.Transactions.IsolationLevel.ReadUncommitted,
            IsolationLevel.ReadCommitted => System.Transactions.IsolationLevel.ReadCommitted,
            IsolationLevel.RepeatableRead => System.Transactions.IsolationLevel.RepeatableRead,
            IsolationLevel.Serializable => System.Transactions.IsolationLevel.Serializable,
            IsolationLevel.Snapshot => System.Transactions.IsolationLevel.Snapshot,
            _ => throw new ArgumentOutOfRangeException(nameof(isolationLevel), isolationLevel, null),
        };

        IPostgresTransaction transaction = new PostgresTransaction(level);

        return Task.FromResult(transaction);
    }
}
