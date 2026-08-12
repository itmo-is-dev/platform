using Microsoft.Build.Framework;
using Newtonsoft.Json.Schema.Generation;
using System.Reflection;
using BuildTask = Microsoft.Build.Utilities.Task;

namespace Itmo.Dev.Platform.Options.MSBuild;

public sealed class GenerateJsonSchemaTask : BuildTask
{
    private const string AttributeTypeName = "Itmo.Dev.Platform.Options.OptionsTypeAttribute";

    [Required]
    public required string AssemblyPath { get; set; }

    [Required]
    public required string OutputPath { get; set; }

    public override bool Execute()
    {
        try
        {
            return ExecuteCore();
        }
        catch (Exception e)
        {
            Log.LogWarning("Error while executing task = {0}", e);
        }

        return true;
    }

    private bool ExecuteCore()
    {
        Log.LogWarning("AssemblyPath={0}, OutputPath={1}", AssemblyPath, OutputPath);

        if (Directory.Exists(OutputPath))
        {
            Directory.Delete(OutputPath, recursive: true);
        }

        Directory.CreateDirectory(OutputPath);

        var assembly = Assembly.LoadFile(AssemblyPath);
        var schemaGenerator = new JSchemaGenerator();

        foreach (TypeInfo optionType in assembly.DefinedTypes)
        {
            var attributes = optionType.GetCustomAttributesData();

            if (attributes.Any(attr => attr.AttributeType.FullName is AttributeTypeName) is false)
            {
                continue;
            }

            var schema = schemaGenerator.Generate(optionType);
            File.WriteAllText(Path.Combine(OutputPath, $"{optionType.FullName}.schema.json"), schema.ToString());
        }

        return true;
    }
}
