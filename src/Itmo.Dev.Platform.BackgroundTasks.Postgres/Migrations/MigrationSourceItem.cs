using FluentMigrator.Runner.Initialization;
using Itmo.Dev.Platform.BackgroundTasks.Persistence.Migrations;

namespace Itmo.Dev.Platform.BackgroundTasks.Postgres.Migrations;

internal class MigrationSourceItem : IMigrationSourceItem
{
    public IEnumerable<Type> MigrationTypeCandidates { get; } = new[]
    {
        typeof(Initial),
        typeof(AddedSuspensionStateValues),
    };
}