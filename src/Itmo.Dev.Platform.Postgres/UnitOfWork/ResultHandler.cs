using Npgsql;

namespace Itmo.Dev.Platform.Postgres.UnitOfWork;

public class ResultHandler : IUnitOfWorkResultHandler
{
    private readonly Func<NpgsqlDataReader, CancellationToken, Task> _func;

    private ResultHandler(Func<NpgsqlDataReader, CancellationToken, Task> func)
    {
        _func = func;
    }

    public static IUnitOfWorkResultHandler Create(Func<NpgsqlDataReader, CancellationToken, Task> func)
        => new ResultHandler(func);

    public Task HandleReaderAsync(NpgsqlDataReader reader, CancellationToken cancellationToken)
    {
        return _func.Invoke(reader, cancellationToken);
    }
}