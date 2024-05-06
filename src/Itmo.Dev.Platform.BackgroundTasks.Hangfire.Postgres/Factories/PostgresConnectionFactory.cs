using Hangfire.PostgreSql;
using Npgsql;

namespace Itmo.Dev.Platform.BackgroundTasks.Hangfire.Postgres.Factories;

internal class PostgresConnectionFactory : IConnectionFactory
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgresConnectionFactory(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public NpgsqlConnection GetOrCreateConnection() => _dataSource.CreateConnection();
}