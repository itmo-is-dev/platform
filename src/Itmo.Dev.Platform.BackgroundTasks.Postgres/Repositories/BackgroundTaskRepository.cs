using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using Itmo.Dev.Platform.BackgroundTasks.Postgres.Queries;
using Itmo.Dev.Platform.BackgroundTasks.Registry;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Itmo.Dev.Platform.Common.Models;
using Itmo.Dev.Platform.Persistence.Abstractions.Connections;
using Itmo.Dev.Platform.Persistence.Postgres.Extensions;
using Newtonsoft.Json;
using System.Data;
using System.Runtime.CompilerServices;

namespace Itmo.Dev.Platform.BackgroundTasks.Postgres.Repositories;

internal class BackgroundTaskRepository : IBackgroundTaskInfrastructureRepository
{
    private readonly BackgroundTaskQueryStorage _queryStorage;
    private readonly JsonSerializerSettings _serializerSettings;
    private readonly IBackgroundTaskRegistry _backgroundTaskRegistry;
    private readonly IPersistenceConnectionProvider _connectionProvider;

    public BackgroundTaskRepository(
        BackgroundTaskQueryStorage queryStorage,
        IPersistenceConnectionProvider connectionProvider,
        JsonSerializerSettings serializerSettings,
        IBackgroundTaskRegistry backgroundTaskRegistry)
    {
        _queryStorage = queryStorage;
        _connectionProvider = connectionProvider;
        _serializerSettings = serializerSettings;
        _backgroundTaskRegistry = backgroundTaskRegistry;
    }

    public async IAsyncEnumerable<BackgroundTask> QueryAsync(
        BackgroundTaskQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        var sql = query.OrderDirection is OrderDirection.Descending
            ? _queryStorage.QueryDescending
            : _queryStorage.QueryAscending;

        await using var command = connection.CreateCommand(sql.Value)
            .AddParameter("ids", query.Ids.Select(x => x.Value).ToArray())
            .AddParameter("names", query.Names)
            .AddParameter("states", query.States)
            .AddJsonArrayParameter("metadata", query.Metadatas, _serializerSettings)
            .AddJsonArrayParameter("execution_metadata", query.ExecutionMetadatas, _serializerSettings)
            .AddParameter("cursor", query.Cursor)
            .AddParameter("max_scheduled_at", query.MaxScheduledAt)
            .AddParameter("page_size", query.PageSize ?? int.MaxValue);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var name = reader.GetString("background_task_name");
            var record = _backgroundTaskRegistry[name];

            var metadata = reader.GetString("background_task_metadata");
            var executionMetadata = reader.GetString("background_task_execution_metadata");
            var resultValue = reader.GetNullableString("background_task_result");
            var errorValue = reader.GetNullableString("background_task_error");

            var deserializedMetadata =
                Deserialize<IBackgroundTaskMetadata>(metadata, record.MetadataType)
                ?? throw new InvalidOperationException("Failed to deserialize metadata");

            var deserializedExecutionMetadata =
                Deserialize<IBackgroundTaskExecutionMetadata>(executionMetadata, record.ExecutionMetadataType)
                ?? throw new InvalidOperationException("Failed to deserialize execution metadata");

            yield return new BackgroundTask(
                Id: new BackgroundTaskId(reader.GetInt64("background_task_id")),
                Name: name,
                Type: record.TaskType,
                CreatedAt: reader.GetFieldValue<DateTimeOffset>("background_task_created_at"),
                ScheduledAt: reader.GetFieldValue<DateTimeOffset?>("background_task_scheduled_at"),
                State: reader.GetFieldValue<BackgroundTaskState>("background_task_state"),
                RetryNumber: reader.GetInt32("background_task_retry_number"),
                Metadata: deserializedMetadata,
                ExecutionMetadata: deserializedExecutionMetadata,
                Result: Deserialize<IBackgroundTaskResult>(resultValue, record.ResultType),
                Error: Deserialize<IBackgroundTaskError>(errorValue, record.ErrorType));
        }
    }

    public async IAsyncEnumerable<BackgroundTaskId> SearchIdsAsync(
        BackgroundTaskQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using var command = connection.CreateCommand(_queryStorage.SearchIds.Value)
            .AddParameter("ids", query.Ids.Select(x => x.Value).ToArray())
            .AddParameter("names", query.Names)
            .AddParameter("states", query.States)
            .AddJsonArrayParameter("metadata", query.Metadatas, _serializerSettings)
            .AddParameter("max_scheduled_at", query.MaxScheduledAt)
            .AddParameter("cursor", query.Cursor)
            .AddParameter("page_size", query.PageSize ?? int.MaxValue);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new BackgroundTaskId(Value: reader.GetInt64("background_task_id"));
        }
    }

    public async IAsyncEnumerable<BackgroundTaskId> AddRangeAsync(
        IReadOnlyCollection<BackgroundTask> tasks,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using var command = connection.CreateCommand(_queryStorage.AddRange.Value)
            .AddParameter("names", tasks.Select(x => x.Name).ToArray())
            .AddParameter("types", tasks.Select(x => x.Type.AssemblyQualifiedName).ToArray())
            .AddParameter("scheduled_at", tasks.Select(x => x.ScheduledAt))
            .AddParameter("created_at", tasks.Select(x => x.CreatedAt).ToArray())
            .AddJsonArrayParameter("metadata", tasks.Select(x => x.Metadata), _serializerSettings)
            .AddJsonArrayParameter("execution_metadata", tasks.Select(x => x.ExecutionMetadata), _serializerSettings);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new BackgroundTaskId(reader.GetInt64(0));
        }
    }

    public async Task UpdateStateAsync(
        IEnumerable<BackgroundTaskId> ids,
        BackgroundTaskState state,
        CancellationToken cancellationToken)
    {
        await using var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using var command = connection.CreateCommand(_queryStorage.UpdateState.Value)
            .AddParameter("ids", ids.Select(x => x.Value).ToArray())
            .AddParameter("state", state);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateAsync(BackgroundTask backgroundTask, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using var command = connection.CreateCommand(_queryStorage.Update.Value)
            .AddParameter("id", backgroundTask.Id.Value)
            .AddParameter("state", backgroundTask.State)
            .AddParameter("retry_number", backgroundTask.RetryNumber)
            .AddJsonParameter("execution_metadata", backgroundTask.ExecutionMetadata, _serializerSettings)
            .AddNullableJsonParameter("result", backgroundTask.Result, _serializerSettings)
            .AddNullableJsonParameter("error", backgroundTask.Error, _serializerSettings)
            .AddParameter("scheduled_at", backgroundTask.ScheduledAt);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private T? Deserialize<T>(string? value, Type type)
        where T : class
    {
        if (value is null)
            return null;

        return JsonConvert.DeserializeObject(value, type, _serializerSettings) as T;
    }
}
