using FluentMigrator;
using Itmo.Dev.Platform.MessagePersistence.Configuration;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Configuration;
using Itmo.Dev.Platform.Persistence.Postgres.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Postgres.Migrations;

[Migration(1706633740, "Init message persistence")]
internal class Initial : SqlMigration
{
    protected override string GetUpSql(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<MessagePersistencePostgresOptions>>();
        
        return $"""
        create schema {options.Value.SchemaName};
        
        create type persisted_message_state as enum
        (
            'pending',
            'completed',
            'failed'
        );

        create table {options.Value.SchemaName}.persisted_messages
        (
            persisted_message_id bigint primary key generated always as identity ,
            persisted_message_name text not null ,
            persisted_message_created_at timestamp with time zone not null ,
            persisted_message_state persisted_message_state not null ,
            persisted_message_key jsonb not null ,
            persisted_message_value jsonb not null 
        );

        create index persisted_message_idx on {options.Value.SchemaName}.persisted_messages(persisted_message_name, persisted_message_state)
        """;
    }

    protected override string GetDownSql(IServiceProvider serviceProvider) =>
    """
    
    """;
}