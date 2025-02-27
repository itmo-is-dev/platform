namespace Itmo.Dev.Platform.BackgroundTasks.Postgres.Queries;

/// <summary>
///     As schema is configurable, this class is needed to store queries
///     (and potentially recreate if schema name changes at runtime [highly unlikely]).
///
///     This is needed to avoid redundant string allocations.
/// </summary>
internal class BackgroundTaskQueryStorage(BackgroundTasksQueryFactory factory)
{
    public BackgroundTasksQuery QueryAscending { get; } = factory.Create(
        o => $"""
        select background_task_id,
               background_task_name,
               background_task_created_at,
               background_task_scheduled_at,
               background_task_state,
               background_task_retry_number,
               background_task_metadata,
               background_task_execution_metadata,
               background_task_result,
               background_task_error
        from {o.SchemaName}.background_tasks
        where 
            (cardinality(:ids) = 0 or background_task_id = any (:ids))
            and (cardinality(:names) = 0 or background_task_name = any (:names))
            and (cardinality(:states) = 0 or background_task_state = any (:states))
            and (cardinality(:metadata) = 0 or background_task_metadata @> any (:metadata))
            and (cardinality(:execution_metadata) = 0 or background_task_execution_metadata @> any (:execution_metadata))
            and (:max_scheduled_at is null
                or background_task_scheduled_at is null
                or background_task_scheduled_at <= :max_scheduled_at)
            and background_task_created_at > :cursor
        order by background_task_created_at
        limit :page_size;
        """);

    public BackgroundTasksQuery QueryDescending { get; } = factory.Create(
        o => $"""
        select background_task_id,
               background_task_name,
               background_task_created_at,
               background_task_scheduled_at,
               background_task_state,
               background_task_retry_number,
               background_task_metadata,
               background_task_execution_metadata,
               background_task_result,
               background_task_error
        from {o.SchemaName}.background_tasks
        where 
            (cardinality(:ids) = 0 or background_task_id = any (:ids))
            and (cardinality(:names) = 0 or background_task_name = any (:names))
            and (cardinality(:states) = 0 or background_task_state = any (:states))
            and (cardinality(:metadata) = 0 or background_task_metadata @> any (:metadata))
            and (cardinality(:execution_metadata) = 0 or background_task_execution_metadata @> any (:execution_metadata)) 
            and (:max_scheduled_at is null
                or background_task_scheduled_at is null
                or background_task_scheduled_at <= :max_scheduled_at)
            and background_task_created_at < :cursor
        order by background_task_created_at desc 
        limit :page_size;
        """);

    public BackgroundTasksQuery SearchIds { get; } = factory.Create(
        o => $"""
        select background_task_id
        from {o.SchemaName}.background_tasks
        where 
            (cardinality(:ids) = 0 or background_task_id = any (:ids))
            and (cardinality(:names) = 0 or background_task_name = any (:names))
            and (cardinality(:states) = 0 or background_task_state = any (:states))
            and (cardinality(:metadata) = 0 or background_task_metadata @> any (:metadata))
            and (:max_scheduled_at is null
                or background_task_scheduled_at is null
                or background_task_scheduled_at <= :max_scheduled_at)
            and background_task_created_at >= :cursor
        order by background_task_created_at
        limit :page_size
        for update skip locked;
        """);

    public BackgroundTasksQuery AddRange { get; } = factory.Create(
        o => $"""
        insert into {o.SchemaName}.background_tasks 
        (
            background_task_name,
            background_task_created_at,
            background_task_scheduled_at,
            background_task_metadata,
            background_task_execution_metadata
        )
        select * from unnest(:names, :created_at, :scheduled_at, :metadata, :execution_metadata)
        returning background_task_id;
        """);

    public BackgroundTasksQuery UpdateState { get; } = factory.Create(
        o => $"""
        update {o.SchemaName}.background_tasks
        set background_task_state = :state
        where background_task_id = any (:ids);
        """);

    public BackgroundTasksQuery Update { get; } = factory.Create(
        o => $"""
        update {o.SchemaName}.background_tasks
        set background_task_state = :state,
            background_task_retry_number = :retry_number,
            background_task_execution_metadata = :execution_metadata,
            background_task_result = :result,
            background_task_error = :error,
            background_task_scheduled_at = :scheduled_at
        where background_task_id = :id;
        """);
}