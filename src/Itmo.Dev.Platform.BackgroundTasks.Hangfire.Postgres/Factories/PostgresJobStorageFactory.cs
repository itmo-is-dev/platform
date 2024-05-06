using Hangfire;
using Hangfire.PostgreSql;
using Itmo.Dev.Platform.BackgroundTasks.Hangfire.Factories;
using Npgsql;

namespace Itmo.Dev.Platform.BackgroundTasks.Hangfire.Postgres.Factories;

internal class PostgresJobStorageFactory : IJobStorageFactory
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgresJobStorageFactory(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public JobStorage CreateStorage()
    {
        return new PostgreSqlStorage(new PostgresConnectionFactory(_dataSource));
    }
}