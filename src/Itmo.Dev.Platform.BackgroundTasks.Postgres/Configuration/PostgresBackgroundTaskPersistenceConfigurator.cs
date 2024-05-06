using Itmo.Dev.Platform.BackgroundTasks.Configuration;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using Itmo.Dev.Platform.BackgroundTasks.Postgres.Migrations;
using Itmo.Dev.Platform.BackgroundTasks.Postgres.Plugins;
using Itmo.Dev.Platform.BackgroundTasks.Postgres.Queries;
using Itmo.Dev.Platform.BackgroundTasks.Postgres.Repositories;
using Itmo.Dev.Platform.Common.Lifetime.Extensions;
using Itmo.Dev.Platform.Persistence.Postgres.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.BackgroundTasks.Postgres.Configuration;

public class PostgresBackgroundTaskPersistenceConfigurator : IBackgroundTasksPersistenceConfigurator
{
    private readonly Action<OptionsBuilder<BackgroundTaskPersistenceOptions>> _optionsConfiguration;

    public PostgresBackgroundTaskPersistenceConfigurator(
        Action<OptionsBuilder<BackgroundTaskPersistenceOptions>> optionsConfiguration)
    {
        _optionsConfiguration = optionsConfiguration;
    }

    public void Apply(IServiceCollection collection)
    {
        var builder = collection
            .AddOptions<BackgroundTaskPersistenceOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        _optionsConfiguration.Invoke(builder);

        collection.AddPlatformLifetimeInitializer<BackgroundTasksMigrationPlatformInitializer>();

        collection.AddSingleton<BackgroundTasksQueryFactory>();
        collection.AddSingleton<BackgroundTaskQueryStorage>();

        collection.AddScoped<BackgroundTaskRepository>();

        collection.AddScoped<IBackgroundTaskRepository>(
            provider => provider.GetRequiredService<BackgroundTaskRepository>());

        collection.AddScoped<IBackgroundTaskInfrastructureRepository>(
            provider => provider.GetRequiredService<BackgroundTaskRepository>());

        collection.TryAddEnumerable(
            ServiceDescriptor.Singleton<IPostgresDataSourcePlugin, BackgroundTaskDataSourcePlugin>());
    }
}