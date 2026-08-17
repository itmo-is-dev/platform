using NJsonSchema;

namespace Itmo.Dev.Platform.Options.MSBuild.Tasks.CurrentSchemaGeneration;

public sealed class SchemaPropertyNode(string name)
{
    public string Name { get; } = name;

    public ICollection<Type> SchemaTypes { get; } = [];

    public IDictionary<string, SchemaPropertyNode> Properties { get; } = new Dictionary<string, SchemaPropertyNode>();

    public SchemaPropertyNode GetChildProperty(string name)
    {
        if (Properties.TryGetValue(name, out var childProperty) is false)
        {
            Properties[name] = childProperty = new SchemaPropertyNode(name);
        }

        return childProperty;
    }

    public void ConfigureSchema(JsonSchema currentSchema, SchemaConfigurationContext context)
    {
        var property = currentSchema.Properties[Name] = new JsonSchemaProperty();
        currentSchema.RequiredProperties.Add(Name);

        foreach (Type type in SchemaTypes.OrderBy(type => type.FullName))
        {
            var typeSchema = context.SchemaResolver.HasSchema(type, isIntegerEnumeration: false)
                ? context.SchemaResolver.GetSchema(type, isIntegerEnumeration: false)
                : context.SchemaGenerator.Generate(type, context.SchemaResolver);

            property.AllOf.Add(new JsonSchema { Reference = typeSchema });
        }

        if (Properties.Count is not 0)
        {
            var currentReference = new JsonSchema();
            property.AllOf.Add(currentReference);

            foreach (SchemaPropertyNode propertyNode in Properties.Values.OrderBy(node => node.Name))
            {
                propertyNode.ConfigureSchema(currentReference, context);
            }
        }
    }
}
