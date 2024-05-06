using Hangfire;
using Itmo.Dev.Platform.BackgroundTasks.Configuration;
using Itmo.Dev.Platform.BackgroundTasks.Hangfire.Factories;
using Itmo.Dev.Platform.BackgroundTasks.Hangfire.Services;
using Itmo.Dev.Platform.BackgroundTasks.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Itmo.Dev.Platform.BackgroundTasks.Hangfire.Configuration;

public class HangfireSchedulingConfigurator : IBackgroundTaskSchedulingConfigurator
{
    private readonly Action<IServiceCollection> _action;

    public HangfireSchedulingConfigurator(Action<IServiceCollection> action)
    {
        _action = action;
    }

    public void Apply(IServiceCollection collection)
    {
        _action.Invoke(collection);

        collection.AddScoped<IBackgroundTaskScheduler, HangfireBackgroundTaskScheduler>();

        collection.AddHangfire(
            (sp, hangfire) =>
            {
                var storageFactory = sp.GetRequiredService<IJobStorageFactory>();
                var configuration = sp.GetRequiredService<IOptions<BackgroundTaskHangfireOptions>>();

                hangfire.UseSerializerSettings(sp.GetRequiredService<JsonSerializerSettings>());
                hangfire.UseStorage(storageFactory.CreateStorage());

                hangfire.UseFilter(
                    new AutomaticRetryAttribute
                    {
                        Attempts = configuration.Value.SchedulerRetryCount,
                        DelaysInSeconds = configuration.Value.SchedulerRetryDelays,
                        OnAttemptsExceeded = AttemptsExceededAction.Delete,
                    });
            });

        collection.AddHangfireServer(
            (p, o) =>
            {
                var options = p.GetRequiredService<IOptions<BackgroundTaskHangfireOptions>>().Value;

                o.WorkerCount = options.SchedulerWorkerCount < 1 ? 1 : options.SchedulerWorkerCount;
                o.CancellationCheckInterval = options.CancellationCheckDelay;
            });
    }
}