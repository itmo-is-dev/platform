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

    private Lazy<string> _querySql;
    private Lazy<string> _searchIdsSql;
    private Lazy<string> _addRangeSql;
    private Lazy<string> _updateStateSql;
    private Lazy<string> _updateSql;

    public BackgroundTaskRepositoryQueryStorage(IOptionsMonitor<BackgroundTaskPersistenceOptions> options)
    {
        _disposable = options.OnChange(OnOptionsChanged);

        _schemaName = options.CurrentValue.SchemaName;

        _querySql = new Lazy<string>(CreateQuerySql);
        _searchIdsSql = new Lazy<string>(CreateSearchIdsSql);
        _addRangeSql = new Lazy<string>(CreateAddRangeSql);
        _updateStateSql = new Lazy<string>(CreateUpdateStateSql);
        _updateSql = new Lazy<string>(CreateUpdateSql);
    }

    public string QuerySql => _querySql.Value;
    public string SearchIdsSql => _searchIdsSql.Value;
    public string AddRangeSql => _addRangeSql.Value;
    public string UpdateStateSql => _updateStateSql.Value;
    public string UpdateSql => _updateSql.Value;

    public void Dispose()
    {
        _disposable?.Dispose();
    }

    private string CreateQuerySql()
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
            and (:metadata is null or background_task_metadata @> :metadata)
            and background_task_created_at >= :cursor
        order by background_task_created_at
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
            and (:metadata is null or background_task_metadata @> :metadata)
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

        _querySql = _querySql.RecreateIfComputed(CreateQuerySql);
        _searchIdsSql = _searchIdsSql.RecreateIfComputed(CreateSearchIdsSql);
        _addRangeSql = _searchIdsSql.RecreateIfComputed(CreateAddRangeSql);
        _updateStateSql = _updateStateSql.RecreateIfComputed(CreateUpdateStateSql);
        _updateSql = _updateSql.RecreateIfComputed(CreateUpdateSql);
    }
}