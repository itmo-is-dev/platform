using Itmo.Dev.Platform.Postgres.Connection;
using Itmo.Dev.Platform.Postgres.UnitOfWork.Handlers;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Concurrent;
using System.Data;

namespace Itmo.Dev.Platform.Postgres.UnitOfWork;

public class ReusableUnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ConcurrentQueue<IWorkHandler> _queue;
    private readonly SemaphoreSlim _semaphore;
    private readonly IPostgresConnectionProvider _connectionProvider;
    private readonly ILogger<ReusableUnitOfWork> _logger;

    public ReusableUnitOfWork(IPostgresConnectionProvider connectionProvider, ILogger<ReusableUnitOfWork> logger)
    {
        _connectionProvider = connectionProvider;
        _logger = logger;
        _queue = new ConcurrentQueue<IWorkHandler>();
        _semaphore = new SemaphoreSlim(1, 1);
    }

    public void Enqueue(NpgsqlCommand command)
    {
        _queue.Enqueue(new NonQueryWorkHandler(command));
    }

    public void Enqueue(NpgsqlCommand command, IUnitOfWorkResultHandler handler)
    {
        _queue.Enqueue(new ReaderWorkHandler(command, handler));
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
            while (count is not 0 && _queue.TryDequeue(out IWorkHandler? handler))
            {
                count--;
                await handler.HandleAsync(connection, cancellationToken);
            }

            transaction.Commit();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to commit work");
            
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