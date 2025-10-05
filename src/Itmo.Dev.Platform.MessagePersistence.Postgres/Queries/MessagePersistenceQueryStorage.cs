namespace Itmo.Dev.Platform.MessagePersistence.Postgres.Queries;

internal class MessagePersistenceQueryStorage(MessagePersistenceQueryFactory factory) : IDisposable
{
    public MessagePersistenceQuery Query { get; } = factory.Create(o => $"""
    select  persisted_message_id,
            persisted_message_name,
            persisted_message_created_at,
            persisted_message_state,
            persisted_message_key,
            persisted_message_value,
            persisted_message_retry_count,
            persisted_message_buffering_step
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
        persisted_message_created_at,
        persisted_message_state,
        persisted_message_key,
        persisted_message_value,
        persisted_message_buffering_step)
    select * from unnest(:names, :created_at, :states, :keys, :values, :buffering_steps)
    """);

    public MessagePersistenceQuery Update { get; } = factory.Create(o => $"""
    update {o.SchemaName}.persisted_messages
    set persisted_message_state = source.state,
        persisted_message_retry_count = source.retry_count,
        persisted_message_buffering_step = source.buffering_step
    from (select * from unnest(:ids, :states, :retry_counts, :buffering_steps)) as source(id, state, retry_count, buffering_step)
    where persisted_message_id = source.id
    """);

    public void Dispose()
    {
        Query.Dispose();
        Add.Dispose();
        Update.Dispose();
    }
}
