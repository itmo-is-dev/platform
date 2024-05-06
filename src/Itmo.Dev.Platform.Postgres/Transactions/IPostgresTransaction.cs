namespace Itmo.Dev.Platform.Postgres.Transactions;

public interface IPostgresTransaction : IAsyncDisposable
{
    ValueTask CommitAsync(CancellationToken cancellationToken);
}