using Itmo.Dev.Platform.Persistence.Postgres.Models;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Persistence.Postgres.Connections;

internal class PostgresConnectionStringProvider : IPostgresConnectionStringProvider
{
    private readonly IDisposable? _subscription;
    private string _connectionString;

    public PostgresConnectionStringProvider(IOptionsMonitor<PostgresConnectionOptions> configuration)
    {
        _connectionString = configuration.CurrentValue.ToConnectionString();
        _subscription = configuration.OnChange(o => _connectionString = o.ToConnectionString());
    }

    public string GetConnectionString() => _connectionString;
}