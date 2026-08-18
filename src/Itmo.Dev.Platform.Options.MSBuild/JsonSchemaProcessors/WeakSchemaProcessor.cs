using NJsonSchema.Generation;

namespace Itmo.Dev.Platform.Options.MSBuild.JsonSchemaProcessors;

public sealed class WeakSchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        context.Schema.RequiredProperties.Clear();
    }
}
