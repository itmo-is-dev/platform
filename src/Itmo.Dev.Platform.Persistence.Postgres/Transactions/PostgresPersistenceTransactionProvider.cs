using Itmo.Dev.Platform.Persistence.Abstractions.Transactions;
using IsolationLevel = System.Data.IsolationLevel;

namespace Itmo.Dev.Platform.Persistence.Postgres.Transactions;

internal class PostgresPersistenceTransactionProvider : IPersistenceTransactionProvider
{
    public ValueTask<IPersistenceTransaction> BeginTransactionAsync(
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

        var transaction = new PostgresPersistenceTransaction(level);
        return ValueTask.FromResult<IPersistenceTransaction>(transaction);
    }
}