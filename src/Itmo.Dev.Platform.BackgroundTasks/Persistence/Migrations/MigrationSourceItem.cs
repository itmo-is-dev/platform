using FluentMigrator.Runner.Initialization;

namespace Itmo.Dev.Platform.BackgroundTasks.Persistence.Migrations;

internal class MigrationSourceItem : IMigrationSourceItem
{
    public IEnumerable<Type> MigrationTypeCandidates { get; } = new[]
    {
        typeof(Initial),
        typeof(AddedSuspensionStateValues),
    };
}