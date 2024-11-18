using FluentMigrator;
using Itmo.Dev.Platform.MessagePersistence.Configuration;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Configuration;
using Itmo.Dev.Platform.Persistence.Postgres.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Postgres.Migrations;

[Migration(1707846055, "Added partitioning")]
public class AddedPartitioning : SqlMigration
{
    protected override string GetUpSql(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<MessagePersistencePostgresOptions>>();
        
        return $"""
        alter table {options.Value.SchemaName}.persisted_messages
        rename to persisted_messages_old;
        
        create table {options.Value.SchemaName}.persisted_messages
        (like {options.Value.SchemaName}.persisted_messages_old including constraints including defaults including identity)
        partition by list (persisted_message_state);
        
        create table {options.Value.SchemaName}.persisted_messages_pending
        (like {options.Value.SchemaName}.persisted_messages including constraints including defaults);
        
        create table {options.Value.SchemaName}.persisted_messages_failed
        (like {options.Value.SchemaName}.persisted_messages including constraints including defaults);
        
        create table {options.Value.SchemaName}.persisted_messages_completed
        (like {options.Value.SchemaName}.persisted_messages including constraints including defaults);
        
        alter table {options.Value.SchemaName}.persisted_messages    
        attach partition {options.Value.SchemaName}.persisted_messages_pending for values in ('pending');
        
        alter table {options.Value.SchemaName}.persisted_messages
        attach partition {options.Value.SchemaName}.persisted_messages_failed for values in ('failed');
        
        alter table {options.Value.SchemaName}.persisted_messages
        attach partition {options.Value.SchemaName}.persisted_messages_completed for values in ('completed');
        
        insert into {options.Value.SchemaName}.persisted_messages overriding system value 
        select * from {options.Value.SchemaName}.persisted_messages_old ;
       """;
    }

    protected override string GetDownSql(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<MessagePersistencePostgresOptions>>();
        
        return $"""
        alter table {options.Value.SchemaName}.persisted_messages
        detach partition {options.Value.SchemaName}.persisted_messages_pending;
        
        alter table {options.Value.SchemaName}.persisted_messages
        detach partition {options.Value.SchemaName}.persisted_messages_failed;
                
        alter table {options.Value.SchemaName}.persisted_messages
        detach partition {options.Value.SchemaName}.persisted_messages_completed;
        """;
    }
}