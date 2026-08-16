using Itmo.Dev.Platform.Options.MSBuild.Tools;
using Microsoft.Build.Framework;
using Newtonsoft.Json.Schema.Generation;
using System.Reflection;
using BuildTask = Microsoft.Build.Utilities.Task;

namespace Itmo.Dev.Platform.Options.MSBuild.Tasks;

public sealed class ProcessOptionTypesBuildTask : BuildTask
{
    private const string AttributeTypeName = "Itmo.Dev.Platform.Options.OptionsTypeAttribute";

    [Required]
    public required string AssemblyPath { get; set; }

    [Required]
    public required string[] SharedFrameworkPaths { get; set; }
    
    [Required]
    public required string[] ReferencePaths { get; set; }

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
            Log.LogWarning("Error while executing task {0} = {1}", nameof(ProcessOptionTypesBuildTask), e);
        }

        return true;
    }

    private bool ExecuteCore()
    {
        Log.LogMessage("AssemblyPath={0}, OutputPath={1}", AssemblyPath, OutputPath);

        Directory.CreateDirectory(OutputPath);

        var assemblyDirectory = Path.GetDirectoryName(AssemblyPath) ?? string.Empty;
        Log.LogMessage("Assembly base dir = {0}", assemblyDirectory);

        var context = new CustomAssemblyLoadContext(
            AssemblyPath,
            SharedFrameworkPaths,
            ReferencePaths,
            Log);

        var assembly = context.LoadFromAssemblyPath(AssemblyPath);

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
