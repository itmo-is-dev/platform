using FluentMigrator;
using Itmo.Dev.Platform.MessagePersistence.Configuration;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Configuration;
using Itmo.Dev.Platform.Persistence.Postgres.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Postgres.Migrations;

[Migration(1707842678, "Added retry count")]
public class AddedRetryCount : SqlMigration
{
    protected override string GetUpSql(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<MessagePersistencePostgresOptions>>();
        
        return $"""
        alter table {options.Value.SchemaName}.persisted_messages
        add column persisted_message_retry_count int default 0;
        """;
    }


    protected override string GetDownSql(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<MessagePersistencePostgresOptions>>();
        
        return $"""
        alter table {options.Value.SchemaName}.persisted_message
        drop column persisted_message_retry_count;
        """;
    }
}