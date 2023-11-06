using Npgsql;

namespace Itmo.Dev.Platform.Postgres.Plugins;

public interface IDataSourcePlugin
{
    void Configure(NpgsqlDataSourceBuilder builder);
}