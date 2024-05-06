using Itmo.Dev.Platform.BackgroundTasks.Hangfire.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.BackgroundTasks.Hangfire.Configuration;

internal class BackgroundTaskHangfireConfigurator :
    IBackgroundTaskHangfireOptionsConfigurator,
    IBackgroundTaskHangfireJobStorageConfigurator,
    IBackgroundTaskHangfireConfigurator
{
    private readonly IServiceCollection _collection;

    public BackgroundTaskHangfireConfigurator(IServiceCollection collection)
    {
        _collection = collection;
    }

    public IBackgroundTaskHangfireJobStorageConfigurator ConfigureOptions(
        Action<OptionsBuilder<BackgroundTaskHangfireOptions>> action)
    {
        var builder = _collection
            .AddOptions<BackgroundTaskHangfireOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        action.Invoke(builder);

        return this;
    }

    public IBackgroundTaskHangfireConfigurator UseJobStorageFactory<T>()
        where T : class, IJobStorageFactory
    {
        _collection.AddSingleton<IJobStorageFactory, T>();
        return this;
    }
}