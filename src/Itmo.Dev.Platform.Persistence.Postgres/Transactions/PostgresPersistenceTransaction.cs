using Itmo.Dev.Platform.Persistence.Abstractions.Transactions;
using System.Transactions;

namespace Itmo.Dev.Platform.Persistence.Postgres.Transactions;

internal class PostgresPersistenceTransaction : IPersistenceTransaction
{
    private readonly TransactionScope _scope;

    public PostgresPersistenceTransaction(IsolationLevel isolationLevel)
    {
        _scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = isolationLevel },
            TransactionScopeAsyncFlowOption.Enabled);
    }

    public ValueTask CommitAsync(CancellationToken cancellationToken)
    {
        _scope.Complete();
        return ValueTask.CompletedTask;
    }

    public ValueTask RollbackAsync(CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        _scope.Dispose();
        return ValueTask.CompletedTask;
    }
}