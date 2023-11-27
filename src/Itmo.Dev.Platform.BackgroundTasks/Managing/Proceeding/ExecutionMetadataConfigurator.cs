using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;

namespace Itmo.Dev.Platform.BackgroundTasks.Managing.Proceeding;

internal class ExecutionMetadataConfigurator : IExecutionMetadataConfigurator
{
    private readonly BackgroundTaskRunner _runner;
    private readonly BackgroundTaskQuery _query;

    public ExecutionMetadataConfigurator(BackgroundTaskRunner runner, BackgroundTaskQuery query)
    {
        _runner = runner;
        _query = query;
    }

    public IProceedTaskRequest WithExecutionMetadataModification<TExecutionMetadata>(
        Action<TExecutionMetadata> modification)
        where TExecutionMetadata : IBackgroundTaskExecutionMetadata
    {
        var modificationObject = new ExecutionMetadataModification(metadata =>
        {
            if (metadata is not TExecutionMetadata executionMetadata)
            {
                var message =
                    $"Invalid metadata type. Expected: {typeof(TExecutionMetadata)}, got: {metadata.GetType()}";

                return new ExecutionMetadataModificationResult.Failure(message);
            }

            modification.Invoke(executionMetadata);
            return new ExecutionMetadataModificationResult.Success(executionMetadata);
        });

        return new ProceedTaskRequest(_runner, _query, modificationObject);
    }

    public IProceedTaskRequest WithExecutionMetadataModification<TExecutionMetadata>(
        Func<TExecutionMetadata, TExecutionMetadata> modification)
        where TExecutionMetadata : IBackgroundTaskExecutionMetadata
    {
        var modificationObject = new ExecutionMetadataModification(metadata =>
        {
            if (metadata is not TExecutionMetadata executionMetadata)
            {
                var message =
                    $"Invalid metadata type. Expected: {typeof(TExecutionMetadata)}, got: {metadata.GetType()}";

                return new ExecutionMetadataModificationResult.Failure(message);
            }

            executionMetadata = modification.Invoke(executionMetadata);
            return new ExecutionMetadataModificationResult.Success(executionMetadata);
        });

        return new ProceedTaskRequest(_runner, _query, modificationObject);
    }

    public IProceedTaskRequest WithoutExecutionMetadataModification()
    {
        var modification = new ExecutionMetadataModification(
            metadata => new ExecutionMetadataModificationResult.Success(metadata));

        return new ProceedTaskRequest(_runner, _query, modification);
    }
}