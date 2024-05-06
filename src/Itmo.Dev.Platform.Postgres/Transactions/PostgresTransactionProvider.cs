using Itmo.Dev.Platform.Postgres.Connection;
using IsolationLevel = System.Data.IsolationLevel;

namespace Itmo.Dev.Platform.Postgres.Transactions;

public class PostgresTransactionProvider : IPostgresTransactionProvider
{
    private readonly IPostgresConnectionProvider _connectionProvider;

    public PostgresTransactionProvider(IPostgresConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task<IPostgresTransaction> CreateTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken)
    {
        // var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);
        // return await connection.BeginTransactionAsync(isolationLevel, cancellationToken);

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

        return new PostgresTransaction(level);
    }
}