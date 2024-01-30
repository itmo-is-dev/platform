namespace Itmo.Dev.Platform.MessagePersistence.Persistence.Queries;

internal class MessagePersistenceQueryStorage(MessagePersistenceQueryFactory factory) : IDisposable
{
    public MessagePersistenceQuery Query { get; } = factory.Create(o => $"""
    select  persisted_message_id,
            persisted_message_name,
            persisted_message_created_at,
            persisted_message_state,
            persisted_message_key,
            persisted_message_value
    from {o.SchemaName}.persisted_messages
    where 
        (persisted_message_name = :message_name)
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
        persisted_message_value)
    select * from unnest(:names, :created_at, :states, :keys, :values)
    """);

    public MessagePersistenceQuery UpdateStates { get; } = factory.Create(o => $"""
    update {o.SchemaName}.persisted_messages
    set persisted_message_state = source.state
    from (select * from unnest(:ids, :states)) as source(id, state)
    where persisted_message_id = source.id
    """);

    public void Dispose()
    {
        Query.Dispose();
        Add.Dispose();
        UpdateStates.Dispose();
    }
}