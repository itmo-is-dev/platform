using Itmo.Dev.Platform.Options.MSBuild.Tools;
using Microsoft.Build.Framework;
using NJsonSchema;
using BuildTask = Microsoft.Build.Utilities.Task;

namespace Itmo.Dev.Platform.Options.MSBuild.Tasks;

public sealed class GenerateCurrentSchemaBuildTask : BuildTask
{
    private const string AttributeTypeName = "Itmo.Dev.Platform.Options.OptionRegistrationAttribute";

    [Required]
    public required string TargetFramework { get; set; }

    [Required]
    public required string AssemblyPath { get; set; }

    [Required]
    public required string[] SharedFrameworkPaths { get; set; }

    [Required]
    public required string ProjectAssetsFilePath { get; set; }

    [Required]
    public required string[] SchemasPaths { get; set; }

    [Required]
    public required string OutputPath { get; set; }

    public override bool Execute()
    {
        var optionRegistrations = EnumerateRegistrations()
            .DistinctBy(x => x.Section)
            .ToArray();

        var registeredOptionTypes = optionRegistrations.Select(x => x.Type).ToHashSet();

        Log.LogMessage("Found '{0}' option registrations", optionRegistrations.Length);

        if (optionRegistrations is [])
            return true;

        var relevantSchemas = EnumerateSchemas()
            .IntersectBy(registeredOptionTypes, schema => schema.TypeName)
            .ToArray();

        Log.LogMessage("Found '{0}' relevant schemas", relevantSchemas.Length);

        var schema = new JsonSchema
        {
            SchemaVersion = "https://json-schema.org/draft/2020-12/schema",
            Type = JsonObjectType.Object,
            ExtensionData = new Dictionary<string, object?>(),
        };

        foreach (OptionsTypeSchema typeSchema in relevantSchemas)
        {
            var sourceTypeSchema = JsonSchema.FromJsonAsync(typeSchema.Schema).GetAwaiter().GetResult();

            foreach (KeyValuePair<string, JsonSchema> definition in sourceTypeSchema.Definitions)
            {
                schema.Definitions.TryAdd(definition.Key, definition.Value);
            }

            schema.Definitions[FormatTypeName(typeSchema.TypeName)] = sourceTypeSchema;
        }

        var propertyNodes = BuildPropertyNodes(optionRegistrations);

        foreach (PropertyNode propertyNode in propertyNodes)
        {
            propertyNode.ConfigureSchema(rootSchema: schema, currentSchema: schema);
        }

        File.WriteAllText(
            Path.Combine(OutputPath, $"{Path.GetFileNameWithoutExtension(AssemblyPath)}.schema.json"),
            schema.ToJson());

        return true;
    }

    private IEnumerable<OptionRegistration> EnumerateRegistrations()
    {
        Log.LogMessage("Loading assembly at '{0}'", AssemblyPath);

        var context = new CustomAssemblyLoadContext(
            TargetFramework,
            AssemblyPath,
            SharedFrameworkPaths,
            ProjectAssetsFilePath,
            Log);

        var assembly = context.LoadFromAssemblyPath(AssemblyPath);

        return assembly
            .GetCustomAttributesData()
            .Where(attr => attr.AttributeType.FullName is AttributeTypeName)
            .Select(attr => new OptionRegistration(
                (string)attr.ConstructorArguments[0].Value!,
                (string)attr.ConstructorArguments[1].Value!));
    }

    private IEnumerable<OptionsTypeSchema> EnumerateSchemas()
    {
        var schemaTypeNames = new HashSet<string>();

        foreach (string schemasPath in SchemasPaths)
        {
            Log.LogMessage("Loading schemas from '{0}'", schemasPath);
            var schemasDirectory = new DirectoryInfo(schemasPath);

            if (schemasDirectory.Exists is false)
            {
                yield break;
            }

            var schemaFiles = schemasDirectory.EnumerateFiles(searchPattern: "*.schema.json");

            foreach (FileInfo schemaFile in schemaFiles)
            {
                var schemaTypeName = schemaFile.Name.Replace(
                    ".schema.json",
                    string.Empty,
                    StringComparison.OrdinalIgnoreCase);

                if (schemaTypeNames.Add(schemaTypeName) is false)
                    continue;

                Log.LogMessage("Found schema for '{0}'", schemaTypeName);

                yield return new OptionsTypeSchema(
                    schemaTypeName,
                    Schema: File.ReadAllText(schemaFile.FullName));
            }
        }
    }

    private IEnumerable<PropertyNode> BuildPropertyNodes(IEnumerable<OptionRegistration> registrations)
    {
        var properties = new Dictionary<string, PropertyNode>();

        foreach (OptionRegistration registration in registrations)
        {
            var pathParts = registration.Section.Split(
                separator: ":",
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            PropertyNode? currentProperty = null;

            foreach (string pathPart in pathParts)
            {
                if (currentProperty is null)
                {
                    if (properties.TryGetValue(pathPart, out currentProperty) is false)
                        currentProperty = properties[pathPart] = new PropertyNode(pathPart);
                }
                else
                {
                    currentProperty = currentProperty.GetChildProperty(pathPart);
                }
            }

            currentProperty?.SchemaTypeNames.Add(registration.Type);
        }

        return properties.Values;
    }

    private readonly record struct OptionsTypeSchema(string TypeName, string Schema);

    private readonly record struct OptionRegistration(string Section, string Type);

    private class PropertyNode(string name)
    {
        public string Name { get; } = name;

        public ICollection<string> SchemaTypeNames { get; } = [];

        public IDictionary<string, PropertyNode> Properties { get; } = new Dictionary<string, PropertyNode>();

        public PropertyNode GetChildProperty(string name)
        {
            if (Properties.TryGetValue(name, out var childProperty) is false)
            {
                Properties[name] = childProperty = new PropertyNode(name);
            }

            return childProperty;
        }

        public void ConfigureSchema(JsonSchema rootSchema, JsonSchema currentSchema)
        {
            var property = currentSchema.Properties[Name] = new JsonSchemaProperty();
            currentSchema.RequiredProperties.Add(Name);

            foreach (string schemaTypeName in SchemaTypeNames)
            {
                if (rootSchema.Definitions.TryGetValue(FormatTypeName(schemaTypeName), out var definition))
                {
                    property.AllOf.Add(new JsonSchema { Reference = definition });
                }
            }

            if (Properties.Count is not 0)
            {
                var currentReference = new JsonSchema();
                property.AllOf.Add(currentReference);

                foreach (KeyValuePair<string, PropertyNode> propertyNode in Properties)
                {
                    propertyNode.Value.ConfigureSchema(rootSchema, currentReference);
                }
            }
        }
    }

    private static string FormatTypeName(string typeName) => typeName.Replace('.', '_');
}
