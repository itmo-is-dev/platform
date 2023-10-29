using Npgsql;

namespace Itmo.Dev.Platform.Postgres.UnitOfWork.Handlers;

internal interface IWorkHandler
{
    Task HandleAsync(NpgsqlConnection connection, CancellationToken cancellationToken);
}