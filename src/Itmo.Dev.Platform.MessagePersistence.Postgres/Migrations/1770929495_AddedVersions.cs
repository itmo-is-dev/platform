using FluentMigrator;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Configuration;
using Itmo.Dev.Platform.Persistence.Postgres.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Postgres.Migrations;

[Migration(1770929495, "Added versions")]
public sealed class AddedVersions : SqlMigration
{
    protected override string GetUpSql(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<MessagePersistencePostgresOptions>>();
                                
        return $"""
        ALTER TABLE {options.Value.SchemaName}.persisted_messages
        ADD COLUMN persisted_message_version bigint NOT NULL DEFAULT 0;
        """;
    }

    protected override string GetDownSql(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<MessagePersistencePostgresOptions>>();

        return $"""
        ALTER TABLE {options.Value.SchemaName}.persisted_messages
        DROP COLUMN persisted_message_version;
        """;
    }
}
