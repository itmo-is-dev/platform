using FluentMigrator.Runner.Initialization;

namespace Itmo.Dev.Platform.MessagePersistence.Postgres.Migrations;

internal class MigrationSourceItem : IMigrationSourceItem
{
    public IEnumerable<Type> MigrationTypeCandidates { get; } = new[]
    {
        typeof(Initial),
        typeof(AddedRetryCount),
        typeof(AddedPartitioning),
    };
}