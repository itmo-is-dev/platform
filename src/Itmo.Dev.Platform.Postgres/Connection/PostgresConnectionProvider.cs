using Itmo.Dev.Platform.Postgres.Models;
using Npgsql;
using System.Data.Common;

namespace Itmo.Dev.Platform.Postgres.Connection;

internal class PostgresConnectionProvider : IPostgresConnectionProvider, IAsyncDisposable, IDisposable
{
    private readonly Lazy<Task<DbConnection>> _connection;

    public PostgresConnectionProvider(PostgresConnectionString connectionString)
    {
        _connection = new Lazy<Task<DbConnection>>(async () =>
        {
            var connection = new NpgsqlConnection(connectionString.Value);
            await connection.OpenAsync();

            return connection;
        });
    }

    public async ValueTask<DbConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        return await _connection.Value;
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection.IsValueCreated is false)
            return;

        var connection = await _connection.Value;
        await connection.DisposeAsync();
    }

    public void Dispose()
    {
        Wrapper().GetAwaiter().GetResult();

        async Task Wrapper()
            => await DisposeAsync();
    }
}