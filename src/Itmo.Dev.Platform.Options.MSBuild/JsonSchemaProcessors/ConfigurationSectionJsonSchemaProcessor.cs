using Namotion.Reflection;
using NJsonSchema.Generation;

namespace Itmo.Dev.Platform.Options.MSBuild.JsonSchemaProcessors;

public sealed class ConfigurationSectionJsonSchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        foreach (ContextualPropertyInfo property in context.ContextualType.Properties)
        {
            if (property.PropertyType.Type.FullName is not "Microsoft.Extensions.Configuration.IConfigurationSection")
                continue;

            if (context.Schema.Properties.TryGetValue(property.Name, out var propertySchema) is false)
                continue;

            propertySchema.OneOf.Clear();
            propertySchema.Reference = null;
        }

        context.Schema.Definitions.Remove("IConfigurationSection");
    }
}
