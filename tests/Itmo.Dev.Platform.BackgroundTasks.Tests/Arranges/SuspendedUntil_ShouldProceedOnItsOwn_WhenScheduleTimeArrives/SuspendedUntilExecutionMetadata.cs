using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.SuspendedUntil_ShouldProceedOnItsOwn_WhenScheduleTimeArrives;

public class SuspendedUntilExecutionMetadata : IBackgroundTaskExecutionMetadata
{
    public bool WasSuspended { get; set; }
}
