using Itmo.Dev.Platform.Postgres.Connection;
using Npgsql;
using System.Collections.Concurrent;
using System.Data;

namespace Itmo.Dev.Platform.Postgres.UnitOfWork;

public class ReusableUnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ConcurrentQueue<NpgsqlCommand> _queue;
    private readonly SemaphoreSlim _semaphore;
    private readonly IPostgresConnectionProvider _connectionProvider;

    public ReusableUnitOfWork(IPostgresConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
        _queue = new ConcurrentQueue<NpgsqlCommand>();
        _semaphore = new SemaphoreSlim(1, 1);
    }

    public void Enqueue(NpgsqlCommand command)
    {
        _queue.Enqueue(command);
    }

    public async ValueTask CommitAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken)
    {
        if (_queue.Count is 0)
            return;

        await _semaphore.WaitAsync(cancellationToken);

        var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        IDbTransaction transaction = await connection
            .BeginTransactionAsync(isolationLevel, cancellationToken);

        int count = _queue.Count;

        try
        {
            while (count is not 0 && _queue.TryDequeue(out NpgsqlCommand? command))
            {
                count--;
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();

            // flushing remaining queue work if transaction failed
            while (count is not 0 && _queue.TryDequeue(out _))
            {
                count--;
            }
        }
        finally
        {
            transaction.Dispose();
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }
}