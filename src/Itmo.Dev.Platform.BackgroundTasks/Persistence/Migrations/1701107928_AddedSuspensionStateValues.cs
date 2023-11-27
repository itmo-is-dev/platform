using FluentMigrator;
using Itmo.Dev.Platform.Postgres.Migrations;

namespace Itmo.Dev.Platform.BackgroundTasks.Persistence.Migrations;

[Migration(1701107928, "Added suspension state cases")]
public class AddedSuspensionStateValues : SqlMigration 
{
    protected override string GetUpSql(IServiceProvider serviceProvider) =>
    """
    alter type background_task_state add value 'suspended';
    alter type background_task_state add value 'proceeded';
    """;

    protected override string GetDownSql(IServiceProvider serviceProvider) =>
    """
    create type background_task_state_tmp as enum
    (
        'pending',
        'enqueued',
        'retrying',
        'failed',
        'cancelled',
        'completed'
    );

    delete from background_tasks
    where background_task_state in ('suspended', 'proceeded');

    alter table background_tasks
        alter column background_task_state type background_task_state_tmp using background_task_state::background_task_state_tmp;

    drop type background_task_state;

    alter type background_task_state_tmp
        rename to background_task_state;
    """;
}