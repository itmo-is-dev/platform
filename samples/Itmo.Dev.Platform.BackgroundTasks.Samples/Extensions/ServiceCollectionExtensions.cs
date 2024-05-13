using Itmo.Dev.Platform.BackgroundTasks.Extensions;
using Itmo.Dev.Platform.BackgroundTasks.Hangfire.Extensions;
using Itmo.Dev.Platform.BackgroundTasks.Hangfire.Postgres.Extensions;
using Itmo.Dev.Platform.BackgroundTasks.Postgres.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.BackgroundTasks.Samples.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterBackgroundTasksSample(
        this IServiceCollection collection,
        IConfiguration configuration)
    {
        return collection.AddPlatformBackgroundTasks(
            tasks => tasks
                .UsePostgresPersistence(
                    builder => builder.BindConfiguration("Infrastructure:BackgroundTasks:Persistence"))
                .ConfigureScheduling(
                    builder => builder.BindConfiguration("Infrastructure:BackgroundTasks:Scheduling"))
                .UseHangfireScheduling(
                    hangfire => hangfire
                        .ConfigureOptions(
                            builder => builder.BindConfiguration("Infrastructure:BackgroundTasks:Scheduling:Hangfire"))
                        .UsePostgresJobStorage())
                .ConfigureExecution(configuration.GetSection("Infrastructure:BackgroundTasks:Execution"))
                .AddStateMachine());
    }
}