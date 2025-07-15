using FluentMigrator;
using FluentMigrator.Runner.Initialization;
using Itmo.Dev.Platform.Persistence.Postgres.Migrations;
using System.Reflection;

namespace Itmo.Dev.Platform.MessagePersistence.Postgres.Migrations;

internal class MigrationSourceItem : IMigrationSourceItem
{
    private static readonly IEnumerable<Type> MigrationTypes = typeof(MigrationSourceItem).Assembly.DefinedTypes
        .Where(type => type.IsAssignableTo(typeof(SqlMigration)))
        .Where(type => type.IsAbstract is false)
        .Select(type => (type, attribute: type.GetCustomAttribute<MigrationAttribute>()))
        .OrderBy(tuple => tuple.attribute!.Version)
        .Select(tuple => tuple.type)
        .ToArray();

    public IEnumerable<Type> MigrationTypeCandidates => MigrationTypes;
}
