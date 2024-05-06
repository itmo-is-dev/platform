using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.BackgroundTasks.Configuration;

public interface IBackgroundTaskSchedulingConfigurator
{
    void Apply(IServiceCollection collection);
}