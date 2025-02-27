using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.SuspendedUntil_ShouldProceedOnItsOwn_WhenScheduleTimeArrives;

public record SuspendedUntilMetadata(DateTimeOffset ContinuationScheduled) : IBackgroundTaskMetadata;
