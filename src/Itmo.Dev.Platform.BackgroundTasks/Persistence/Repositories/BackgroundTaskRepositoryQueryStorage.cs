using Itmo.Dev.Platform.BackgroundTasks.Configuration;
using Itmo.Dev.Platform.BackgroundTasks.Extensions;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.BackgroundTasks.Persistence.Repositories;

/// <summary>
///     As schema is configurable, this class is needed to store queries
///     (and potentially recreate if schema name changes at runtime [highly unlikely]).
///
///     This is needed to avoid redundant string allocations.
/// </summary>
internal class BackgroundTaskRepositoryQueryStorage : IDisposable
{
    private readonly IDisposable? _disposable;
    private string _schemaName;

    private Lazy<string> _queryAscendingSql;
    private Lazy<string> _queryDescendingSql;
    private Lazy<string> _searchIdsSql;
    private Lazy<string> _addRangeSql;
    private Lazy<string> _updateStateSql;
    private Lazy<string> _updateSql;

    public BackgroundTaskRepositoryQueryStorage(IOptionsMonitor<BackgroundTaskPersistenceOptions> options)
    {
        _disposable = options.OnChange(OnOptionsChanged);

        _schemaName = options.CurrentValue.SchemaName;

        _queryAscendingSql = new Lazy<string>(CreateQueryAscendingSql);
        _queryDescendingSql = new Lazy<string>(CreateQueryDescendingSql);
        _searchIdsSql = new Lazy<string>(CreateSearchIdsSql);
        _addRangeSql = new Lazy<string>(CreateAddRangeSql);
        _updateStateSql = new Lazy<string>(CreateUpdateStateSql);
        _updateSql = new Lazy<string>(CreateUpdateSql);
    }

    public string QueryAscendingSql => _queryAscendingSql.Value;
    public string QueryDescendingSql => _queryDescendingSql.Value;
    public string SearchIdsSql => _searchIdsSql.Value;
    public string AddRangeSql => _addRangeSql.Value;
    public string UpdateStateSql => _updateStateSql.Value;
    public string UpdateSql => _updateSql.Value;

    public void Dispose()
    {
        _disposable?.Dispose();
    }

    private string CreateQueryAscendingSql()
    {
        return $"""
        select background_task_id,
               background_task_name,
               background_task_created_at,
               background_task_state,
               background_task_retry_number,
               background_task_metadata,
               background_task_execution_metadata,
               background_task_result,
               background_task_error
        from {_schemaName}.background_tasks
        where 
            (cardinality(:ids) = 0 or background_task_id = any (:ids))
            and (cardinality(:names) = 0 or background_task_name = any (:names))
            and (cardinality(:states) = 0 or background_task_state = any (:states))
            and (cardinality(:metadata) = 0 or background_task_metadata @> any (:metadata))
            and (cardinality(:execution_metadata) = 0 or background_task_execution_metadata @> any (:execution_metadata)) 
            and background_task_created_at > :cursor
        order by background_task_created_at
        limit :page_size;
        """;
    }

    private string CreateQueryDescendingSql()
    {
        return $"""
        select background_task_id,
               background_task_name,
               background_task_created_at,
               background_task_state,
               background_task_retry_number,
               background_task_metadata,
               background_task_execution_metadata,
               background_task_result,
               background_task_error
        from {_schemaName}.background_tasks
        where 
            (cardinality(:ids) = 0 or background_task_id = any (:ids))
            and (cardinality(:names) = 0 or background_task_name = any (:names))
            and (cardinality(:states) = 0 or background_task_state = any (:states))
            and (cardinality(:metadata) = 0 or background_task_metadata @> any (:metadata))
            and (cardinality(:execution_metadata) = 0 or background_task_execution_metadata @> any (:execution_metadata)) 
            and background_task_created_at < :cursor
        order by background_task_created_at desc 
        limit :page_size;
        """;
    }

    private string CreateSearchIdsSql()
    {
        return $"""
        select background_task_id
        from {_schemaName}.background_tasks
        where 
            (cardinality(:ids) = 0 or background_task_id = any (:ids))
            and (cardinality(:names) = 0 or background_task_name = any (:names))
            and (cardinality(:states) = 0 or background_task_state = any (:states))
            and (cardinality(:metadata) = 0 or background_task_metadata @> any (:metadata))
            and background_task_created_at >= :cursor
        order by background_task_created_at
        limit :page_size
        for update skip locked;
        """;
    }

    private string CreateAddRangeSql()
    {
        return $"""
        insert into {_schemaName}.background_tasks 
        (
            background_task_name,
            background_task_created_at,
            background_task_metadata,
            background_task_execution_metadata
        )
        select * from unnest(:names, :created_at, :metadata, :execution_metadata)
        returning background_task_id;
        """;
    }

    private string CreateUpdateStateSql()
    {
        return $"""
        update {_schemaName}.background_tasks
        set background_task_state = :state
        where background_task_id = any (:ids);
        """;
    }

    private string CreateUpdateSql()
    {
        return $"""
        update {_schemaName}.background_tasks
        set background_task_state = :state,
            background_task_retry_number = :retry_number,
            background_task_execution_metadata = :execution_metadata,
            background_task_result = :result,
            background_task_error = :error
        where background_task_id = :id;
        """;
    }

    private void OnOptionsChanged(BackgroundTaskPersistenceOptions options)
    {
        _schemaName = options.SchemaName;

        _queryAscendingSql = _queryAscendingSql.RecreateIfComputed(CreateQueryAscendingSql);
        _queryDescendingSql = _queryDescendingSql.RecreateIfComputed(CreateQueryDescendingSql);
        _searchIdsSql = _searchIdsSql.RecreateIfComputed(CreateSearchIdsSql);
        _addRangeSql = _searchIdsSql.RecreateIfComputed(CreateAddRangeSql);
        _updateStateSql = _updateStateSql.RecreateIfComputed(CreateUpdateStateSql);
        _updateSql = _updateSql.RecreateIfComputed(CreateUpdateSql);
    }
}