using Itmo.Dev.Platform.BackgroundTasks.Configuration;
using Itmo.Dev.Platform.BackgroundTasks.Postgres.Configuration;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.BackgroundTasks.Postgres.Extensions;

public static class BackgroundTaskConfigurationBuilderExtensions
{
    public static IBackgroundTaskSchedulingOptionsConfigurator UsePostgresPersistence(
        this IBackgroundTaskPersistenceConfigurationSelector selector,
        Action<OptionsBuilder<BackgroundTaskPersistenceOptions>> options)
    {
        var configurator = new PostgresBackgroundTaskPersistenceConfigurator(options);
        return selector.UsePersistenceConfigurator(configurator);
    }
}