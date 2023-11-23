using Itmo.Dev.Platform.Postgres.Models;
using Itmo.Dev.Platform.Postgres.Plugins;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Itmo.Dev.Platform.Postgres.Connection;

internal class PostgresConnectionProvider : IPostgresConnectionProvider, IAsyncDisposable, IDisposable
{
    private readonly Lazy<Task<NpgsqlConnection>> _connection;
    private readonly ILogger<PostgresConnectionProvider> _logger;

    public PostgresConnectionProvider(
        PostgresConnectionString connectionString,
        IEnumerable<IDataSourcePlugin> plugins,
        ILogger<PostgresConnectionProvider> logger)
    {
        _logger = logger;
        _connection = new Lazy<Task<NpgsqlConnection>>(async () =>
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString.Value);

            foreach (IDataSourcePlugin plugin in plugins)
            {
                plugin.Configure(dataSourceBuilder);
            }

            var dataSource = dataSourceBuilder.Build();
            
            _logger.LogTrace("Opening connection");

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
        {
            _logger.LogTrace("Connection was not created");
            return;
        }
        
        _logger.LogTrace("Disposing connection");

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