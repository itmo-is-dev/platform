using Itmo.Dev.Platform.Postgres.Connection;
using Npgsql;
using System.Data;

namespace Itmo.Dev.Platform.Testing.Mocks;

public class PostgresConnectionProviderMock : IPostgresConnectionProvider, IAsyncDisposable
{
    private readonly Lazy<Task<NpgsqlConnection>> _connection;
    private readonly ConnectionState _connectionState;

    public PostgresConnectionProviderMock(NpgsqlConnection connection)
    {
        _connectionState = connection.State;

        _connection = new Lazy<Task<NpgsqlConnection>>(async () =>
        {
            if (_connectionState is not ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            return connection;
        });
    }

    public async ValueTask<NpgsqlConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        return await _connection.Value;
    }

    public async ValueTask DisposeAsync()
    {
        if (_connectionState is not ConnectionState.Open && _connection.IsValueCreated)
        {
            var connection = await _connection.Value;
            await connection.CloseAsync();
        }
    }
}