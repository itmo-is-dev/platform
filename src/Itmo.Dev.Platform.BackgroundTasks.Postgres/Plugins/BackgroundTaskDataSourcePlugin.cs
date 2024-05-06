using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.Persistence.Postgres.Plugins;
using Npgsql;

namespace Itmo.Dev.Platform.BackgroundTasks.Postgres.Plugins;

internal class BackgroundTaskDataSourcePlugin : IPostgresDataSourcePlugin
{
    public void Configure(NpgsqlDataSourceBuilder builder)
    {
        builder.MapEnum<BackgroundTaskState>();
    }
}