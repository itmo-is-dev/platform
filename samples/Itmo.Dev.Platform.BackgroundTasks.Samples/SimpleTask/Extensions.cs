using Itmo.Dev.Platform.BackgroundTasks.Configuration;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;

namespace Itmo.Dev.Platform.BackgroundTasks.Samples.SimpleTask;

public static class Extensions
{
    public static void AddSimpleBackgroundTask(this IBackgroundTaskConfigurationBuilder builder)
    {
        builder.AddBackgroundTask(b => b
            .WithMetadata<SimpleTaskMetadata>()
            .WithExecutionMetadata<EmptyExecutionMetadata>()
            .WithResult<SimpleTaskResult>()
            .WithError<EmptyError>()
            .HandleBy<SimpleBackgroundTask>());
    }
}