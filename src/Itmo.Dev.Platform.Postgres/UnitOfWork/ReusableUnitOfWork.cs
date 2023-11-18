using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.Postgres.Connection;
using Itmo.Dev.Platform.Postgres.Exceptions;
using Itmo.Dev.Platform.Postgres.Extensions;
using Itmo.Dev.Platform.Postgres.UnitOfWork.Handles;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Concurrent;
using System.Data;

namespace Itmo.Dev.Platform.Postgres.UnitOfWork;

public class ReusableUnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ConcurrentQueue<IWorkHandle> _queue;
    private readonly SemaphoreSlim _semaphore;
    private readonly IPostgresConnectionProvider _connectionProvider;
    private readonly ILogger<ReusableUnitOfWork> _logger;

    public ReusableUnitOfWork(IPostgresConnectionProvider connectionProvider, ILogger<ReusableUnitOfWork> logger)
    {
        _connectionProvider = connectionProvider;
        _logger = logger;
        _queue = new ConcurrentQueue<IWorkHandle>();
        _semaphore = new SemaphoreSlim(1, 1);
    }

    internal int Count => _queue.Count;

    public void Enqueue(NpgsqlCommand command)
    {
        _queue.Enqueue(new NonQueryWorkHandle(command.ToBatchCommand()));
    }

    public void Enqueue(NpgsqlCommand command, IUnitOfWorkResultHandler handler)
    {
        _queue.Enqueue(new ReaderWorkHandle(command.ToBatchCommand(), handler));
    }

    public async ValueTask CommitAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken)
    {
        if (_queue.Count is 0)
            return;

        using var subscription = await _semaphore.UseAsync(cancellationToken);

        var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        IDbTransaction transaction = await connection
            .BeginTransactionAsync(isolationLevel, cancellationToken);

        int count = _queue.Count;

        try
        {
            var handles = _queue.Take(count).ToArray();

            if (handles.Length != count)
            {
                string message = $"Failed to obtain {count} work handles, found only {handles.Length}";
                throw new PostgresPlatformException(message);
            }

            for (var i = 0; i < count; i++)
            {
                if (_queue.TryDequeue(out _) is false)
                {
                    throw new PostgresPlatformException("Failed to dequeue work handles");
                }
            }

            await using (var batch = new NpgsqlBatch(connection))
            {
                foreach (NpgsqlBatchCommand command in handles.Select(x => x.BatchCommand))
                {
                    batch.BatchCommands.Add(command);
                }

                await using var reader = await batch.ExecuteReaderAsync(cancellationToken);

                foreach (IWorkHandle handle in handles)
                {
                    await handle.HandleResult(reader, cancellationToken);
                }
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

            throw;
        }
        finally
        {
            transaction.Dispose();
        }
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }
}