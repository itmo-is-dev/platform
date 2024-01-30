using FluentMigrator.Runner.Initialization;

namespace Itmo.Dev.Platform.MessagePersistence.Persistence.Migrations;

public class MigrationSourceItem : IMigrationSourceItem
{
    public IEnumerable<Type> MigrationTypeCandidates { get; } = new[]
    {
        typeof(Initial),
    };
}