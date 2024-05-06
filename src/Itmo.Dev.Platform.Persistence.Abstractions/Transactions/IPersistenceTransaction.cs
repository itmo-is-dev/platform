namespace Itmo.Dev.Platform.Persistence.Abstractions.Transactions;

public interface IPersistenceTransaction : IAsyncDisposable
{
    ValueTask CommitAsync(CancellationToken cancellationToken);

    ValueTask RollbackAsync(CancellationToken cancellationToken);
}