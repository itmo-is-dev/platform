using Npgsql;

namespace Itmo.Dev.Platform.Postgres.UnitOfWork.Handlers;

internal class NonQueryWorkHandler : IWorkHandler
{
    private readonly NpgsqlCommand _command;

    public NonQueryWorkHandler(NpgsqlCommand command)
    {
        _command = command;
    }

    public async Task HandleAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        await using var command = _command;
        command.Connection = connection;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}