using Itmo.Dev.Platform.Options.MSBuild.Extensions;
using Itmo.Dev.Platform.Options.MSBuild.Tools;
using Microsoft.Build.Framework;
using Namotion.Reflection;
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
        Log.LogDebugMessage("SharedFrameworkPaths = {0}", string.Join(";", SharedFrameworkPaths));

        var assembly = assemblyLoadContext.LoadFromAssemblyPath(AssemblyPath);

        var optionRegistrations = EnumerateRegistrations(assembly, assemblyLoadContext)
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

    private IEnumerable<OptionRegistration> EnumerateRegistrations(
        Assembly assembly,
        CustomAssemblyLoadContext context)
    {
        Log.LogDebugMessage("Loading registrations for {0}", assembly.GetName().Name);

        var optionsAssembly = context.LoadFromAssemblyName(new AssemblyName("Itmo.Dev.Platform.Options"));
        var optionsAttributeType = optionsAssembly.GetType(OptionRegistrationAttributeTypeName);

        Log.LogDebugMessage("Loaded attribute type = {0}", optionsAttributeType);

        if (optionsAttributeType is null)
            yield break;

        foreach (Attribute attribute in assembly.GetCustomAttributes(optionsAttributeType))
        {
            var sectionName = attribute.TryGetPropertyValue<string>("SectionName");
            var type = attribute.TryGetPropertyValue<Type>("OptionsType");

            if (sectionName is null || type is null)
            {
                Log.LogDebugMessage("Invalid attribute, section name = '{0}', options type = {1}",
                    sectionName,
                    type);
            }
            else
            {
                yield return new OptionRegistration(sectionName, type);
            }
        }
    }
}
