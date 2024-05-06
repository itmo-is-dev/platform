using Itmo.Dev.Platform.BackgroundTasks.Configuration;
using Itmo.Dev.Platform.BackgroundTasks.Execution;
using Itmo.Dev.Platform.BackgroundTasks.Managing;
using Itmo.Dev.Platform.BackgroundTasks.Scheduling;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.BackgroundTasks.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformBackgroundTasks(
        this IServiceCollection collection,
        Func<IBackgroundTaskPersistenceConfigurationSelector, IBackgroundTaskConfigurationBuilder> action)
    {
        var builder = new BackgroundTaskConfigurationBuilder(collection);
        action.Invoke(builder);

        collection.AddScoped<IBackgroundTaskManager, BackgroundTaskExecutionWrapper>();
        collection.AddScoped<IBackgroundTaskRunner, BackgroundTaskRunner>();

        collection.AddHostedService<BackgroundTaskSchedulingService>();

        return collection;
    }
}