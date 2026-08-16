using Namotion.Reflection;
using NJsonSchema.Generation;

namespace Itmo.Dev.Platform.Options.MSBuild.JsonSchemaProcessors;

public sealed class PublicPropertyJsonSchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        foreach (ContextualPropertyInfo property in context.ContextualType.Properties)
        {
            if (property.PropertyInfo.SetMethod?.IsPublic is not false)
                continue;

            context.Schema.Properties.Remove(property.Name);
        }
    }
}
