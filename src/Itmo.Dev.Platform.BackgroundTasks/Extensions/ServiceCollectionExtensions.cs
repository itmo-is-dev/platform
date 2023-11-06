using Hangfire;
using Hangfire.PostgreSql;
using Itmo.Dev.Platform.BackgroundTasks.Configuration;
using Itmo.Dev.Platform.BackgroundTasks.Configuration.Builders;
using Itmo.Dev.Platform.BackgroundTasks.Execution;
using Itmo.Dev.Platform.BackgroundTasks.Managing;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using Itmo.Dev.Platform.BackgroundTasks.Persistence.Plugins;
using Itmo.Dev.Platform.BackgroundTasks.Persistence.Repositories;
using Itmo.Dev.Platform.BackgroundTasks.Scheduling;
using Itmo.Dev.Platform.Postgres.Models;
using Itmo.Dev.Platform.Postgres.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Itmo.Dev.Platform.BackgroundTasks.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformBackgroundTasks(
        this IServiceCollection collection,
        Func<IBackgroundTaskPersistenceConfigurator, IBackgroundTaskConfigurationBuilder> action)
    {
        var builder = new BackgroundTaskConfigurationBuilder(collection);
        action.Invoke(builder);

        collection.AddScoped<IBackgroundTaskExecutor, BackgroundTaskExecutionWrapper>();
        collection.AddScoped<IBackgroundTaskRunner, BackgroundTaskRunner>();

        collection.AddSingleton<BackgroundTaskRepositoryQueryStorage>();
        collection.AddScoped<BackgroundTaskRepository>();
        collection.AddScoped<IBackgroundTaskRepository>(p => p.GetRequiredService<BackgroundTaskRepository>());

        collection.AddSingleton<IDataSourcePlugin, BackgroundTaskDataSourcePlugin>();

        collection.AddScoped<IBackgroundTaskInfrastructureRepository>(
            p => p.GetRequiredService<BackgroundTaskRepository>());

        collection.AddHostedService<BackgroundTaskSchedulingService>();
        collection.AddScoped<IBackgroundTaskScheduler, HangfireBackgroundTaskScheduler>();

        collection.AddHangfire((sp, hangfire) =>
        {
            var connectionString = sp.GetRequiredService<PostgresConnectionString>();
            var configuration = sp.GetRequiredService<IOptions<BackgroundTaskSchedulingOptions>>();

            hangfire.UseSerializerSettings(sp.GetRequiredService<JsonSerializerSettings>());
            hangfire.UsePostgreSqlStorage(x => x.UseNpgsqlConnection(connectionString.Value));

            hangfire.UseFilter(new AutomaticRetryAttribute
            {
                Attempts = configuration.Value.SchedulerRetryCount,
                DelaysInSeconds = configuration.Value.SchedulerRetryDelays,
            });
        });

        collection.AddHangfireServer();

        return collection;
    }
}