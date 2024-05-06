using Itmo.Dev.Platform.BackgroundTasks.Hangfire.Factories;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.BackgroundTasks.Hangfire.Configuration;

public interface IBackgroundTaskHangfireOptionsConfigurator
{
    IBackgroundTaskHangfireJobStorageConfigurator ConfigureOptions(
        Action<OptionsBuilder<BackgroundTaskHangfireOptions>> action);
}

public interface IBackgroundTaskHangfireJobStorageConfigurator
{
    IBackgroundTaskHangfireConfigurator UseJobStorageFactory<T>()
        where T : class, IJobStorageFactory;
}

public interface IBackgroundTaskHangfireConfigurator { }