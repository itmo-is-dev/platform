using FluentMigrator;
using Itmo.Dev.Platform.BackgroundTasks.Postgres.Configuration;
using Itmo.Dev.Platform.Persistence.Postgres.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.BackgroundTasks.Postgres.Migrations;

[Migration(1740681513, "Added scheduled_at")]
internal class AddedScheduledAt : BackgroundTasksMigration 
{
    protected override string GetUpSql(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<BackgroundTaskPersistenceOptions>>();
        
        return $"""
        alter table {options.Value.SchemaName}.background_tasks
        add column background_task_scheduled_at timestamp with time zone null;
        """;
    }

    protected override string GetDownSql(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<BackgroundTaskPersistenceOptions>>();
        
        return $"""
        alter table {options.Value.SchemaName}.background_tasks
        drop column background_task_scheduled_at;
        """;
    }
}
