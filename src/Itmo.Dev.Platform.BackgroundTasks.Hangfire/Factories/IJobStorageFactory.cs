using Hangfire;

namespace Itmo.Dev.Platform.BackgroundTasks.Hangfire.Factories;

public interface IJobStorageFactory
{
    JobStorage CreateStorage();
}