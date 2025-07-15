using FluentMigrator;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Configuration;
using Itmo.Dev.Platform.Persistence.Postgres.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Postgres.Migrations;

[Migration(1750877758, "Added published partition")]
public class RemoveOldTable : SqlMigration
{
    protected override string GetUpSql(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<MessagePersistencePostgresOptions>>();
        
        return $"""
        drop table {options.Value.SchemaName}.persisted_messages_old;

        create table {options.Value.SchemaName}.persisted_messages_published
        (like {options.Value.SchemaName}.persisted_messages including constraints including defaults);
        
        alter table {options.Value.SchemaName}.persisted_messages
        attach partition {options.Value.SchemaName}.persisted_messages_published for values in ('published');
        """;
    }

    protected override string GetDownSql(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<MessagePersistencePostgresOptions>>();

        return $"""
        drop table {options.Value.SchemaName}.persisted_messages_published;
        """;
    }
}
