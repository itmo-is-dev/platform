using Itmo.Dev.Platform.BackgroundTasks.Configuration.Builders;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.Samples.SuspendedTask;

public static class Extensions
{
    public static void AddSuspendedTask(this IBackgroundTaskConfigurationBuilder builder)
    {
        builder.AddBackgroundTask(b => b
            .WithMetadata<SuspendedTaskMetadata>()
            .WithExecutionMetadata<SuspendedTaskExecutionMetadata>()
            .WithResult<EmptyExecutionResult>()
            .WithError<EmptyError>()
            .HandleBy<SuspendedBackgroundTask>());
    }
}