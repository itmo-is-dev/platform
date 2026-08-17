using NJsonSchema.Generation;

namespace Itmo.Dev.Platform.Options.MSBuild.Tools;

public sealed class FullNameSchemaNameGenerator : ISchemaNameGenerator
{
    public string Generate(Type type)
    {
        return type.FullName?.Replace('.', '_') ?? throw new InvalidOperationException("Type does not have full name");
    }
}
