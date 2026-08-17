using Namotion.Reflection;
using NJsonSchema;
using NJsonSchema.Generation;

namespace Itmo.Dev.Platform.Options.MSBuild.JsonSchemaProcessors;

public sealed class ConfigurationSectionJsonSchemaProcessor : ISchemaProcessor
{
    private const string ConfigurationSectionTypeName = "Microsoft.Extensions.Configuration.IConfigurationSection";
    private static readonly string ConfigurationSectionDefinitionName = ConfigurationSectionTypeName.Replace('.', '_');

    public void Process(SchemaProcessorContext context)
    {
        foreach (ContextualPropertyInfo property in context.ContextualType.Properties)
        {
            if (property.PropertyType.Type.FullName is not ConfigurationSectionTypeName)
                continue;

            if (context.Schema.Properties.TryGetValue(property.Name, out var propertySchema) is false)
                continue;

            propertySchema.OneOf.Clear();
            propertySchema.Reference = null;
        }

        if (context.Resolver.RootObject is JsonSchema rootSchema)
            rootSchema.Definitions.Remove(ConfigurationSectionDefinitionName);
    }
}
