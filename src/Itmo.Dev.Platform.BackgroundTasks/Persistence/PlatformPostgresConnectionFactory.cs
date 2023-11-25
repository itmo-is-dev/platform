using Hangfire.PostgreSql;
using Npgsql;

namespace Itmo.Dev.Platform.BackgroundTasks.Persistence;

internal class PlatformPostgresConnectionFactory : IConnectionFactory
{
    private readonly NpgsqlDataSource _dataSource;

    public PlatformPostgresConnectionFactory(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public NpgsqlConnection GetOrCreateConnection()
    {
        return _dataSource.CreateConnection();
    }
}