using Itmo.Dev.Platform.BackgroundTasks.Configuration;
using Itmo.Dev.Platform.BackgroundTasks.Postgres.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

    public static IBackgroundTaskSchedulingOptionsConfigurator UsePostgresPersistence(
        IBackgroundTaskPersistenceConfigurationSelector selector,
        string sectionPath)
    {
        var configurator = new PostgresBackgroundTaskPersistenceConfigurator(
            builder => builder.BindConfiguration(sectionPath));

        return selector.UsePersistenceConfigurator(configurator);
    }
}