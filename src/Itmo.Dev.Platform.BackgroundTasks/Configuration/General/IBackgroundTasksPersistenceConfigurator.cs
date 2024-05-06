using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.BackgroundTasks.Configuration;

public interface IBackgroundTasksPersistenceConfigurator
{
    void Apply(IServiceCollection collection);
}