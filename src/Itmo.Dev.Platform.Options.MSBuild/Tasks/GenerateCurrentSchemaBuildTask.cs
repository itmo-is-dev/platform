using Itmo.Dev.Platform.Options.MSBuild.Tools;
using Microsoft.Build.Framework;
using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.Generation;
using System.Reflection;
using BuildTask = Microsoft.Build.Utilities.Task;

namespace Itmo.Dev.Platform.Options.MSBuild.Tasks;

public sealed class GenerateCurrentSchemaBuildTask : BuildTask
{
    private const string OptionRegistrationAttributeTypeName = "Itmo.Dev.Platform.Options.OptionRegistrationAttribute";
    private const string OptionsTypeAttributeTypeName = "Itmo.Dev.Platform.Options.OptionsTypeAttribute";

    [Required]
    public required string TargetFramework { get; set; }

    [Required]
    public required string AssemblyPath { get; set; }

    [Required]
    public required string[] SharedFrameworkPaths { get; set; }

    [Required]
    public required string ProjectAssetsFilePath { get; set; }

    [Required]
    public required string OutputPath { get; set; }

    public override bool Execute()
    {
        using var assemblyLoadContext = new CustomAssemblyLoadContext(
            TargetFramework,
            AssemblyPath,
            SharedFrameworkPaths,
            ProjectAssetsFilePath,
            Log);

        Log.LogMessage("Loading assembly at '{0}'", AssemblyPath);

        var assembly = assemblyLoadContext.LoadFromAssemblyPath(AssemblyPath);

        var optionRegistrations = EnumerateRegistrations(assembly)
            .DistinctBy(x => x.Section)
            .ToArray();

        Log.LogMessage("Found '{0}' option registrations", optionRegistrations.Length);

        if (optionRegistrations is [])
            return true;

        var schema = new JsonSchema
        {
            SchemaVersion = "https://json-schema.org/draft/2020-12/schema",
            Type = JsonObjectType.Object,
            ExtensionData = new Dictionary<string, object?>(),
        };

        var jsonSchemaSettings = JsonSchemaSettings.CreateDefault();
        var jsonSchemaResolver = new JsonSchemaResolver(schema, jsonSchemaSettings);
        var jsonSchemaGenerator = new JsonSchemaGenerator(jsonSchemaSettings);

        foreach (Type optionsType in EnumerateOptionsTypes(assembly, assemblyLoadContext).OrderBy(type => type.Name))
        {
            if (string.IsNullOrEmpty(optionsType.FullName))
                continue;

            if (jsonSchemaResolver.HasSchema(optionsType, false))
                continue;

            jsonSchemaGenerator.Generate(optionsType, jsonSchemaResolver);
        }

        var propertyNodes = BuildPropertyNodes(optionRegistrations).OrderBy(node => node.Name);

        foreach (PropertyNode propertyNode in propertyNodes)
        {
            propertyNode.ConfigureSchema(rootSchema: schema, currentSchema: schema);
        }

        File.WriteAllText(OutputPath, schema.ToJson());

        return true;
    }

    private IEnumerable<OptionRegistration> EnumerateRegistrations(Assembly assembly)
    {
        return assembly
            .GetCustomAttributesData()
            .Where(attr => attr.AttributeType.FullName is OptionRegistrationAttributeTypeName)
            .Select(attr => new OptionRegistration(
                (string)attr.ConstructorArguments[0].Value!,
                (string)attr.ConstructorArguments[1].Value!));
    }

    private IEnumerable<Type> EnumerateOptionsTypes(
        Assembly assembly,
        CustomAssemblyLoadContext assemblyLoadContext)
    {
        var processedAssemblies = new HashSet<string>();

        var assemblyQueue = new Queue<Assembly>();
        assemblyQueue.Enqueue(assembly);

        while (assemblyQueue.TryDequeue(out var currentAssembly))
        {
            foreach (TypeInfo type in currentAssembly.DefinedTypes)
            {
                var attributes = type.GetCustomAttributesData();

                if (attributes.All(attribute => attribute.AttributeType.FullName is not OptionsTypeAttributeTypeName))
                    continue;

                yield return type;
            }

            foreach (AssemblyName referencedAssemblyName in currentAssembly.GetReferencedAssemblies())
            {
                if (string.IsNullOrEmpty(referencedAssemblyName.Name))
                    continue;

                if (processedAssemblies.Add(referencedAssemblyName.Name) is false)
                    continue;

                if (assemblyLoadContext.TryLoadFromAssemblyName(referencedAssemblyName, out var referencedAssembly))
                    assemblyQueue.Enqueue(referencedAssembly);
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

            foreach (string schemaTypeName in SchemaTypeNames.Order())
            {
                if (rootSchema.Definitions.TryGetValue(schemaTypeName.Replace('.', '_'), out var definition))
                {
                    property.AllOf.Add(new JsonSchema { Reference = definition });
                }
            }

            if (Properties.Count is not 0)
            {
                var currentReference = new JsonSchema();
                property.AllOf.Add(currentReference);

                foreach (PropertyNode propertyNode in Properties.Values.OrderBy(node => node.Name))
                {
                    propertyNode.ConfigureSchema(rootSchema, currentReference);
                }
            }
        }
    }
}
