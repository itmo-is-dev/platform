using FluentMigrator;
using FluentMigrator.Runner.Initialization;
using System.Reflection;

namespace Itmo.Dev.Platform.BackgroundTasks.Postgres.Migrations;

internal class MigrationSourceItem : IMigrationSourceItem
{
    private static readonly IEnumerable<Type> MigrationTypes = typeof(MigrationSourceItem).Assembly.DefinedTypes
        .Where(type => type.IsAssignableTo(typeof(BackgroundTasksMigration)))
        .Where(type => type.IsAbstract is false)
        .Select(type => (type, attribute: type.GetCustomAttribute<MigrationAttribute>()))
        .OrderBy(tuple => tuple.attribute!.Version)
        .Select(tuple => tuple.type)
        .ToArray();

    public IEnumerable<Type> MigrationTypeCandidates => MigrationTypes;
}
