using Npgsql;

namespace Itmo.Dev.Platform.Postgres.UnitOfWork.Handles;

internal interface IWorkHandle
{
    NpgsqlBatchCommand BatchCommand { get; }

    ValueTask HandleResult(NpgsqlDataReader reader, CancellationToken cancellationToken);
}