using Hangfire.PostgreSql;
using Itmo.Dev.Platform.Postgres.Connection;
using Npgsql;

namespace Itmo.Dev.Platform.BackgroundTasks.Persistence;

internal class PlatformPostgresConnectionFactory : IConnectionFactory
{
    private readonly IPostgresConnectionFactory _connectionFactory;

    public PlatformPostgresConnectionFactory(IPostgresConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public NpgsqlConnection GetOrCreateConnection()
    {
        return _connectionFactory.CreateConnection();
    }
}