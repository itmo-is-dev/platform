using Microsoft.Build.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Reflection;
using BuildTask = Microsoft.Build.Utilities.Task;

namespace Itmo.Dev.Platform.Options.MSBuild;

public sealed class GenerateFinalSchemaTask : BuildTask
{
    private const string AttributeTypeName = "Itmo.Dev.Platform.Options.OptionRegistrationAttribute";

    [Required]
    public required string AssemblyPath { get; set; }

    [Required]
    public required string SchemasPath { get; set; }

    public override bool Execute()
    {
        var schemaFiles = Directory.EnumerateFiles(SchemasPath, "*.schema.json");

        var schemas = schemaFiles
            .Select(path => new TypeSchema(
                Path.GetFileName(path).Replace(".schema.json", string.Empty, StringComparison.OrdinalIgnoreCase),
                File.ReadAllText(path)));

        var schema = new JSchema();

        var definitions = schema.ExtensionData["$defs"] = new JObject();

        foreach (TypeSchema typeSchema in schemas)
        {
            definitions[typeSchema.TypeName.Replace(".", "_")] = JObject.Parse(typeSchema.Schema);
        }

        var assembly = Assembly.LoadFile(AssemblyPath);

        var registrations = assembly.CustomAttributes
            .Where(attr => attr.AttributeType.FullName is AttributeTypeName)
            .Select(attr => new OptionRegistration(
                (string)attr.ConstructorArguments[0].Value!,
                (string)attr.ConstructorArguments[1].Value!));

        foreach (OptionRegistration registration in registrations)
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

                properties = sectionSchema;
            }

            properties.ExtensionData["$ref"] = $"#/$defs/{registration.Type.Replace(".", "_")}";
        }

        File.WriteAllText(
            Path.Combine(SchemasPath, $"{Path.GetFileNameWithoutExtension(AssemblyPath)}.schema.json"),
            schema.ToString());

        return true;
    }

    private readonly record struct TypeSchema(string TypeName, string Schema);

    private readonly record struct OptionRegistration(string Section, string Type);
}
