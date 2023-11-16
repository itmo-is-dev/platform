using Npgsql;

namespace Itmo.Dev.Platform.Postgres.UnitOfWork.Handles;

internal class ReaderWorkHandle : IWorkHandle
{
    private readonly IUnitOfWorkResultHandler _resultHandler;

    public ReaderWorkHandle(NpgsqlBatchCommand command, IUnitOfWorkResultHandler resultHandler)
    {
        BatchCommand = command;
        _resultHandler = resultHandler;
    }

    public NpgsqlBatchCommand BatchCommand { get; }

    public async ValueTask HandleResult(NpgsqlDataReader reader, CancellationToken cancellationToken)
    {
        await _resultHandler.HandleReaderAsync(reader, cancellationToken);
        await reader.NextResultAsync(cancellationToken);
    }
}