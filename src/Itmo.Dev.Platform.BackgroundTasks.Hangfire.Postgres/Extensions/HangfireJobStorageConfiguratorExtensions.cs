using Itmo.Dev.Platform.BackgroundTasks.Hangfire.Configuration;
using Itmo.Dev.Platform.BackgroundTasks.Hangfire.Postgres.Factories;

namespace Itmo.Dev.Platform.BackgroundTasks.Hangfire.Postgres.Extensions;

public static class HangfireJobStorageConfiguratorExtensions
{
    public static IBackgroundTaskHangfireConfigurator UsePostgresJobStorage(
        this IBackgroundTaskHangfireJobStorageConfigurator configurator)
    {
        return configurator.UseJobStorageFactory<PostgresJobStorageFactory>();
    }
}