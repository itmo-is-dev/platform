using FluentMigrator;
using Itmo.Dev.Platform.BackgroundTasks.Configuration;
using Itmo.Dev.Platform.BackgroundTasks.Postgres.Configuration;
using Itmo.Dev.Platform.BackgroundTasks.Postgres.Migrations;
using Itmo.Dev.Platform.Persistence.Postgres.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.BackgroundTasks.Persistence.Migrations;

[Migration(1699204381, "Initialized background tasks")]
internal class Initial : BackgroundTasksMigration 
{
    protected override string GetUpSql(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<BackgroundTaskPersistenceOptions>>();
        
        return $"""
        create schema {options.Value.SchemaName};

        create type background_task_state as enum
        (
            'pending',
            'enqueued',
            'retrying',
            'failed',
            'cancelled',
            'completed'
        );

        create table {options.Value.SchemaName}.background_tasks
        (
            background_task_id bigint primary key generated always as identity,
            background_task_name text not null ,
            background_task_created_at timestamp with time zone not null ,
            background_task_state background_task_state not null default 'pending',
            background_task_retry_number int not null default 0,
            background_task_metadata jsonb not null ,
            background_task_execution_metadata jsonb not null ,
            background_task_result jsonb ,
            background_task_error jsonb
        );
        """;
    }

    protected override string GetDownSql(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<BackgroundTaskPersistenceOptions>>();
        
        return $"""
        drop schema {options.Value.SchemaName};
        """;
    }
}