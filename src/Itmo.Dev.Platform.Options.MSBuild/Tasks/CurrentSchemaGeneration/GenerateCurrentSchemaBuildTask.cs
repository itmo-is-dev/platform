using Itmo.Dev.Platform.Options.MSBuild.Extensions;
using Itmo.Dev.Platform.Options.MSBuild.Tools;
using Microsoft.Build.Framework;
using NJsonSchema;
using NJsonSchema.Generation;
using System.Reflection;
using BuildTask = Microsoft.Build.Utilities.Task;

namespace Itmo.Dev.Platform.Options.MSBuild.Tasks.CurrentSchemaGeneration;

public sealed class GenerateCurrentSchemaBuildTask : BuildTask
{
    private const string OptionRegistrationAttributeTypeName = "Itmo.Dev.Platform.Options.OptionRegistrationAttribute";

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

    [Required]
    public required bool IsDebug { get; set; }

    public override bool Execute()
    {
        using var _ = LoggingExtensions.UseDebugLogging(IsDebug);
        using var __ = Log.UseDebugScope(nameof(GenerateCurrentSchemaBuildTask));

        using var assemblyLoadContext = new CustomAssemblyLoadContext(
            TargetFramework,
            AssemblyPath,
            SharedFrameworkPaths,
            ProjectAssetsFilePath,
            Log);

        Log.LogDebugMessage("Loading assembly at '{0}'", AssemblyPath);

        var assembly = assemblyLoadContext.LoadFromAssemblyPath(AssemblyPath);

        var optionRegistrations = EnumerateRegistrations(assembly)
            .DistinctBy(x => x.Section)
            .ToArray();

        Log.LogDebugMessage("Found '{0}' option registrations", optionRegistrations.Length);

        if (optionRegistrations is [])
            return true;

        var schema = new JsonSchema
        {
            SchemaVersion = "https://json-schema.org/draft/2020-12/schema",
            Type = JsonObjectType.Object,
            ExtensionData = new Dictionary<string, object?>(),
        };

        var schemaSettings = JsonSchemaSettings.CreateDefault();
        var schemaResolver = new JsonSchemaResolver(schema, schemaSettings);
        var schemaGenerator = new JsonSchemaGenerator(schemaSettings);

        var schemaConfigurationContext = new SchemaConfigurationContext(schemaResolver, schemaGenerator);

        var propertyNodes = SchemaPropertyNodeFactory
            .FromOptionRegistrations(optionRegistrations, Log)
            .OrderBy(node => node.Name);

        foreach (SchemaPropertyNode propertyNode in propertyNodes)
        {
            propertyNode.ConfigureSchema(schema, schemaConfigurationContext);
        }

        File.WriteAllText(OutputPath, schema.ToJson());

        return true;
    }

    private static IEnumerable<OptionRegistration> EnumerateRegistrations(Assembly assembly)
    {
        return assembly
            .GetCustomAttributesData()
            .Where(attr => attr.AttributeType.FullName is OptionRegistrationAttributeTypeName)
            .Select(attr => new OptionRegistration(
                (string)attr.ConstructorArguments[0].Value!,
                (Type)attr.ConstructorArguments[1].Value!));
    }
}
