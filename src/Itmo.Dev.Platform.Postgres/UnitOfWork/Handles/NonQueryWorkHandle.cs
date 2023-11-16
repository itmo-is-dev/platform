using Npgsql;

namespace Itmo.Dev.Platform.Postgres.UnitOfWork.Handles;

internal class NonQueryWorkHandle : IWorkHandle
{
    public NonQueryWorkHandle(NpgsqlBatchCommand batchCommand)
    {
        BatchCommand = batchCommand;
    }

    public NpgsqlBatchCommand BatchCommand { get; }

    public ValueTask HandleResult(NpgsqlDataReader reader, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;
}