using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;

namespace Itmo.Dev.Platform.BackgroundTasks.Managing.Proceeding;

public interface IQueryParameterConfigurator
{
    IExecutionMetadataConfigurator WithId(BackgroundTaskId backgroundTaskId);

    IExecutionMetadataConfigurator WithQuery(BackgroundTaskQuery query);
}

public interface IExecutionMetadataConfigurator
{
    IProceedTaskRequest WithExecutionMetadataModification<TExecutionMetadata>(Action<TExecutionMetadata> modification)
        where TExecutionMetadata : IBackgroundTaskExecutionMetadata;

    IProceedTaskRequest WithExecutionMetadataModification<TExecutionMetadata>(
        Func<TExecutionMetadata, TExecutionMetadata> modification)
        where TExecutionMetadata : IBackgroundTaskExecutionMetadata;

    IProceedTaskRequest WithoutExecutionMetadataModification();
}

public interface IProceedTaskRequest
{
    Task<ProceedTaskResult> ProceedAsync(CancellationToken cancellationToken);

    Task<ProceedTaskResult> ProceedAtAsync(DateTimeOffset scheduledAt, CancellationToken cancellationToken);
}
