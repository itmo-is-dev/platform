using Npgsql;

namespace Itmo.Dev.Platform.Persistence.Postgres.Plugins;

public interface IPostgresDataSourcePlugin
{
    void Configure(NpgsqlDataSourceBuilder dataSource);
}