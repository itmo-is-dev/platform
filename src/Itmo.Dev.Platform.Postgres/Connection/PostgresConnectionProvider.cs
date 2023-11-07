using Itmo.Dev.Platform.Postgres.Models;
using Itmo.Dev.Platform.Postgres.Plugins;
using Npgsql;

namespace Itmo.Dev.Platform.Postgres.Connection;

internal class PostgresConnectionProvider : IPostgresConnectionProvider, IAsyncDisposable, IDisposable
{
    private readonly Lazy<Task<NpgsqlConnection>> _connection;

    public PostgresConnectionProvider(PostgresConnectionString connectionString, IEnumerable<IDataSourcePlugin> plugins)
    {
        _connection = new Lazy<Task<NpgsqlConnection>>(async () =>
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString.Value);

            foreach (IDataSourcePlugin plugin in plugins)
            {
                plugin.Configure(dataSourceBuilder);
            }

            var dataSource = dataSourceBuilder.Build();

            return await dataSource.OpenConnectionAsync();
        });
    }

    public async ValueTask<NpgsqlConnection> GetConnectionAsync(CancellationToken cancellationToken)
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