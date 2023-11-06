using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.Postgres.Plugins;
using Npgsql;

namespace Itmo.Dev.Platform.BackgroundTasks.Persistence.Plugins;

internal class BackgroundTaskDataSourcePlugin : IDataSourcePlugin
{
    public void Configure(NpgsqlDataSourceBuilder builder)
    {
        builder.MapEnum<BackgroundTaskState>();
    }
}