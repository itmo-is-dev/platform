using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Arranges.ScheduleWithAsync_ShouldScheduleTask_OnlyWhenScheduleTimeArrives;

public record ScheduledAtMetadata(DateTimeOffset ScheduledAt) : IBackgroundTaskMetadata;
