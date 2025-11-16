using FluentMigrator;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Configuration;
using Itmo.Dev.Platform.Persistence.Postgres.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Postgres.Migrations;

[Migration(1763246858, "Added message headers")]
public sealed class AddedMessageHeaders : SqlMigration
{
    protected override string GetUpSql(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<MessagePersistencePostgresOptions>>();
        
        return $"""
        ALTER TABLE {options.Value.SchemaName}.persisted_messages
            ADD COLUMN persisted_message_headers jsonb null;
        """;
    }

    protected override string GetDownSql(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<MessagePersistencePostgresOptions>>();
        
        return $"""
        ALTER TABLE {options.Value.SchemaName}.persisted_messages
            DROP COLUMN persisted_message_headers;
        """;
    }
}
