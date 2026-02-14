namespace Itmo.Dev.Platform.MessagePersistence.Postgres.Queries;

internal class MessagePersistenceQueryStorage(MessagePersistenceQueryFactory factory) : IDisposable
{
    public MessagePersistenceQuery Query { get; } = factory.Create(o => $"""
    select  persisted_message_id,
            persisted_message_name,
            persisted_message_version,
            persisted_message_created_at,
            persisted_message_state,
            persisted_message_key,
            persisted_message_value,
            persisted_message_retry_count,
            persisted_message_buffering_step,
            persisted_message_headers
    from {o.SchemaName}.persisted_messages
    where 
        (cardinality(:ids) = 0 or persisted_message_id = any(:ids))
        and (cardinality(:message_names) = 0 or persisted_message_name = any(:message_names))
        and (cardinality(:states) = 0 or persisted_message_state = any (:states))
        and (:ignore_cursor or persisted_message_created_at >= :cursor)
    order by persisted_message_created_at
    limit :page_size
    for update skip locked;
    """);

    public MessagePersistenceQuery Add { get; } = factory.Create(o => $"""
    insert into {o.SchemaName}.persisted_messages
        (persisted_message_name,
         persisted_message_version,
         persisted_message_created_at,
         persisted_message_state,
         persisted_message_key,
         persisted_message_value,
         persisted_message_headers)
    select * from unnest(:names, :versions, :created_at, :states, :keys, :values, :headers)
    returning persisted_message_id;
    """);

    public MessagePersistenceQuery Update { get; } = factory.Create(o => $"""
    update {o.SchemaName}.persisted_messages
    set persisted_message_version = source.version,
        persisted_message_state = source.state,
        persisted_message_retry_count = source.retry_count,
        persisted_message_buffering_step = source.buffering_step,
        persisted_message_headers = source.headers
    from (select * from unnest(:ids, :versions, :states, :retry_counts, :buffering_steps, :headers)) 
            as source(id, version, state, retry_count, buffering_step, headers)
    where persisted_message_id = source.id
    """);

    public void Dispose()
    {
        Query.Dispose();
        Add.Dispose();
        Update.Dispose();
    }
}
