using Itmo.Dev.Platform.BackgroundTasks.Managing.Proceeding;
using Itmo.Dev.Platform.BackgroundTasks.Managing.Running;

namespace Itmo.Dev.Platform.BackgroundTasks.Managing;

public interface IBackgroundTaskRunner
{
    IMetadataConfigurator StartBackgroundTask { get; }
    
    IQueryParameterConfigurator ProceedBackgroundTask { get; }
}