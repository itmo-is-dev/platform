using Microsoft.Extensions.Logging;
using Npgsql;

namespace Itmo.Dev.Platform.Postgres.Connection;

internal class PostgresConnectionProvider : IPostgresConnectionProvider, IAsyncDisposable, IDisposable
{
    private readonly Lazy<Task<NpgsqlConnection>> _connection;
    private readonly ILogger<PostgresConnectionProvider> _logger;
    private readonly IPostgresConnectionFactory _connectionFactory;

    public PostgresConnectionProvider(
        IPostgresConnectionFactory connectionFactory,
        ILogger<PostgresConnectionProvider> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;

        _connection = new Lazy<Task<NpgsqlConnection>>(async () =>
        {
            _logger.LogTrace("Opening connection");
            
            var connection = connectionFactory.CreateConnection();
            await connection.OpenAsync();

            return connection;
        });
    }

    public ValueTask<NpgsqlConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        return _connectionFactory.OpenConnectionAsync(cancellationToken);
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