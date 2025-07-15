using FluentMigrator;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Configuration;
using Itmo.Dev.Platform.Persistence.Postgres.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Postgres.Migrations;

[Migration(1749898545, "Added buffering")]
public class AddedBuffering : SqlMigration
{
    protected override string GetUpSql(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<MessagePersistencePostgresOptions>>();
        
        return $"""
        alter table {options.Value.SchemaName}.persisted_messages
        add column persisted_message_buffering_step text null;

        alter type persisted_message_state add value 'published';
        """;
    }

    protected override string GetDownSql(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<MessagePersistencePostgresOptions>>();

        return $"""
        alter table {options.Value.SchemaName}.persisted_messages
        drop column persisted_message_buffering_step;

        alter type persisted_message_state rename to persisted_message_state_old;
        create type persisted_message_state as enum ('pending', 'completed', 'failed');
        
        delete from {options.Value.SchemaName}.persisted_messages
        where persisted_message_state = 'published';

        alter table {options.Value.SchemaName}.persisted_messages
        alter column persisted_message_state type persisted_message_state using persisted_message_state::text::persisted_message_state;
        """;
    }
}
