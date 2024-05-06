using System.Transactions;

namespace Itmo.Dev.Platform.Postgres.Transactions;

public class PostgresTransaction : IPostgresTransaction
{
    private readonly TransactionScope _scope;

    public PostgresTransaction(IsolationLevel isolationLevel)
    {
        _scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = isolationLevel },
            TransactionScopeAsyncFlowOption.Enabled);
    }

    public ValueTask DisposeAsync()
    {
        _scope.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask CommitAsync(CancellationToken cancellationToken)
    {
        _scope.Complete();
        return ValueTask.CompletedTask;
    }
}