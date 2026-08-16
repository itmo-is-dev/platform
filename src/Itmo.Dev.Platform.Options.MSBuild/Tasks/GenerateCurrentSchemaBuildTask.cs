using Itmo.Dev.Platform.Options.MSBuild.Tools;
using Microsoft.Build.Framework;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using BuildTask = Microsoft.Build.Utilities.Task;

namespace Itmo.Dev.Platform.Options.MSBuild.Tasks;

public sealed class GenerateCurrentSchemaBuildTask : BuildTask
{
    private const string AttributeTypeName = "Itmo.Dev.Platform.Options.OptionRegistrationAttribute";

    [Required]
    public required string AssemblyPath { get; set; }

    [Required]
    public required string[] SharedFrameworkPaths { get; set; }
    
    [Required]
    public required string[] ReferencePaths { get; set; }

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

        var schema = new JSchema();
        schema.SchemaVersion = new Uri("https://json-schema.org/draft/2020-12/schema");

        foreach (OptionRegistration registration in optionRegistrations)
        {
            var parts = registration.Section.Split(
                ":",
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            JSchema properties = schema;

            foreach (string part in parts)
            {
                if (properties.Properties.TryGetValue(part, out var sectionSchema) is false)
                {
                    sectionSchema = properties.Properties[part] = new JSchema();
                }

                properties.Required.Add(part);
                properties = sectionSchema;
            }

            properties.ExtensionData["$ref"] = $"#/$defs/{FormatTypeName(registration.Type)}";
        }

        var definitions = schema.ExtensionData["$defs"] = new JObject();

        foreach (OptionsTypeSchema typeSchema in relevantSchemas)
        {
            definitions[FormatTypeName(typeSchema.TypeName)] = JObject.Parse(typeSchema.Schema);
        }

        File.WriteAllText(
            Path.Combine(OutputPath, $"{Path.GetFileNameWithoutExtension(AssemblyPath)}.schema.json"),
            schema.ToString());

        return true;
    }

    private IEnumerable<OptionRegistration> EnumerateRegistrations()
    {
        Log.LogMessage("Loading assembly at '{0}'", AssemblyPath);

        var context = new CustomAssemblyLoadContext(
            AssemblyPath,
            SharedFrameworkPaths,
            ReferencePaths,
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

    private readonly record struct OptionsTypeSchema(string TypeName, string Schema);

    private readonly record struct OptionRegistration(string Section, string Type);

    private static string FormatTypeName(string typeName) => typeName.Replace('.', '_');
}
