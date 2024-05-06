using Itmo.Dev.Platform.BackgroundTasks.Configuration;
using Itmo.Dev.Platform.BackgroundTasks.Hangfire.Configuration;

namespace Itmo.Dev.Platform.BackgroundTasks.Hangfire.Extensions;

public static class BackgroundTaskConfigurationBuilderExtensions
{
    public static IBackgroundTaskExecutionConfigurator UseHangfireScheduling(
        this IBackgroundTaskSchedulingConfigurationSelector selector,
        Func<IBackgroundTaskHangfireOptionsConfigurator, IBackgroundTaskHangfireConfigurator> configuration)
    {
        var configurator = new HangfireSchedulingConfigurator(
            services =>
            {
                var hangfireConfigurator = new BackgroundTaskHangfireConfigurator(services);
                configuration.Invoke(hangfireConfigurator);
            });

        return selector.UseSchedulingConfigurator(configurator);
    }
}