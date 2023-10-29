using Npgsql;

namespace Itmo.Dev.Platform.Postgres.UnitOfWork.Handlers;

internal class ReaderWorkHandler : IWorkHandler
{
    private readonly NpgsqlCommand _command;
    private readonly IUnitOfWorkResultHandler _resultHandler;

    public ReaderWorkHandler(NpgsqlCommand command, IUnitOfWorkResultHandler resultHandler)
    {
        _command = command;
        _resultHandler = resultHandler;
    }

    public async Task HandleAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        await using var command = _command;
        command.Connection = connection;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await _resultHandler.HandleReaderAsync(reader, cancellationToken);
    }
}