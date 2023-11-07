using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Registry;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Itmo.Dev.Platform.Postgres.Connection;
using Itmo.Dev.Platform.Postgres.Extensions;
using Newtonsoft.Json;
using Npgsql;
using System.Runtime.CompilerServices;

namespace Itmo.Dev.Platform.BackgroundTasks.Persistence.Repositories;

internal class BackgroundTaskRepository : IBackgroundTaskInfrastructureRepository
{
    private readonly BackgroundTaskRepositoryQueryStorage _queryStorage;
    private readonly IPostgresConnectionProvider _connectionProvider;
    private readonly JsonSerializerSettings _serializerSettings;
    private readonly IBackgroundTaskRegistry _backgroundTaskRegistry;

    public BackgroundTaskRepository(
        BackgroundTaskRepositoryQueryStorage queryStorage,
        IPostgresConnectionProvider connectionProvider,
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
        var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

#pragma warning disable CA2100
        await using var command = new NpgsqlCommand(_queryStorage.QuerySql, connection)
            .AddParameter("ids", query.Ids.Select(x => x.Value).ToArray())
            .AddParameter("names", query.Names)
            .AddParameter("states", query.States)
            .AddNullableJsonParameter("metadata", query.Metadata, _serializerSettings)
            .AddParameter("cursor", query.Cursor)
            .AddParameter("page_size", query.PageSize ?? int.MaxValue);
#pragma warning restore CA2100

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var id = reader.GetOrdinal("background_task_id");
        var name = reader.GetOrdinal("background_task_name");
        var createdAt = reader.GetOrdinal("background_task_created_at");
        var state = reader.GetOrdinal("background_task_state");
        var retryNumber = reader.GetOrdinal("background_task_retry_number");
        var metadata = reader.GetOrdinal("background_task_metadata");
        var executionMetadata = reader.GetOrdinal("background_task_execution_metadata");
        var result = reader.GetOrdinal("background_task_result");
        var error = reader.GetOrdinal("background_task_error");

        while (await reader.ReadAsync(cancellationToken))
        {
            var record = _backgroundTaskRegistry[reader.GetString(name)];

            var metadataValue = reader.GetString(metadata);
            var executionMetadataValue = reader.GetString(executionMetadata);
            var resultValue = reader.GetNullableString(result);
            var errorValue = reader.GetNullableString(error);

            var deserializedMetadata =
                Deserialize<IBackgroundTaskMetadata>(metadataValue, record.MetadataType)
                ?? throw new InvalidOperationException("Failed to deserialize metadata");

            var deserializedExecutionMetadata =
                Deserialize<IBackgroundTaskExecutionMetadata>(executionMetadataValue, record.ExecutionMetadataType)
                ?? throw new InvalidOperationException("Failed to deserialize execution metadata");

            yield return new BackgroundTask(
                Id: new BackgroundTaskId(reader.GetInt64(id)),
                Name: reader.GetString(name),
                Type: record.TaskType,
                CreatedAt: reader.GetFieldValue<DateTimeOffset>(createdAt),
                State: reader.GetFieldValue<BackgroundTaskState>(state),
                RetryNumber: reader.GetInt32(retryNumber),
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
        var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

#pragma warning disable CA2100
        await using var command = new NpgsqlCommand(_queryStorage.SearchIdsSql, connection)
            .AddParameter("ids", query.Ids.Select(x => x.Value).ToArray())
            .AddParameter("names", query.Names)
            .AddParameter("states", query.States)
            .AddNullableJsonParameter("metadata", query.Metadata, _serializerSettings)
            .AddParameter("cursor", query.Cursor)
            .AddParameter("page_size", query.PageSize ?? int.MaxValue);
#pragma warning restore CA2100

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var id = reader.GetOrdinal("background_task_id");

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new BackgroundTaskId(Value: reader.GetInt64(id));
        }
    }

    public async IAsyncEnumerable<BackgroundTaskId> AddRangeAsync(
        IReadOnlyCollection<BackgroundTask> tasks,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

#pragma warning disable CA2100
        await using var command = new NpgsqlCommand(_queryStorage.AddRangeSql, connection)
            .AddParameter("names", tasks.Select(x => x.Name).ToArray())
            .AddParameter("types", tasks.Select(x => x.Type.AssemblyQualifiedName).ToArray())
            .AddParameter("created_at", tasks.Select(x => x.CreatedAt).ToArray())
            .AddJsonArrayParameter("metadata", tasks.Select(x => x.Metadata), _serializerSettings)
            .AddJsonArrayParameter("execution_metadata", tasks.Select(x => x.ExecutionMetadata), _serializerSettings);
#pragma warning restore CA2100

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
        var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

#pragma warning disable CA2100
        await using var command = new NpgsqlCommand(_queryStorage.UpdateStateSql, connection)
            .AddParameter("ids", ids.Select(x => x.Value).ToArray())
            .AddParameter("state", state);
#pragma warning restore CA2100

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateAsync(BackgroundTask backgroundTask, CancellationToken cancellationToken)
    {
        var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

#pragma warning disable CA2100
        await using var command = new NpgsqlCommand(_queryStorage.UpdateSql, connection)
            .AddParameter("id", backgroundTask.Id.Value)
            .AddParameter("state", backgroundTask.State)
            .AddParameter("retry_number", backgroundTask.RetryNumber)
            .AddJsonParameter("execution_metadata", backgroundTask.ExecutionMetadata, _serializerSettings)
            .AddNullableJsonParameter("result", backgroundTask.Result, _serializerSettings)
            .AddNullableJsonParameter("error", backgroundTask.Error, _serializerSettings);
#pragma warning restore CA2100

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private T? Deserialize<T>(string? value, Type type) where T : class
    {
        if (value is null)
            return null;

        return JsonConvert.DeserializeObject(value, type, _serializerSettings) as T;
    }
}