using FluentMigrator.Runner.Initialization;

namespace Itmo.Dev.Platform.MessagePersistence.Persistence.Migrations;

internal class MigrationSourceItem : IMigrationSourceItem
{
    public IEnumerable<Type> MigrationTypeCandidates { get; } = new[]
    {
        typeof(Initial),
    };
}