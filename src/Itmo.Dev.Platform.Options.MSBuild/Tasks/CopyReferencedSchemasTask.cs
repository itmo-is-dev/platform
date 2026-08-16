using Microsoft.Build.Framework;
using Newtonsoft.Json.Linq;
using BuildTask = Microsoft.Build.Utilities.Task;

namespace Itmo.Dev.Platform.Options.MSBuild.Tasks;

public sealed class CopyReferencedSchemasTask : BuildTask
{
    [Required]
    public required string ProjectAssetsFile { get; set; }

    [Required]
    public required string TargetDir { get; set; }

    public override bool Execute()
    {
        try
        {
            return ExecuteCore();
        }
        catch (Exception e)
        {
            Log.LogError("Error while executing '{0}' = {1}", nameof(CopyReferencedSchemasTask), e);
            return false;
        }
    }

    private bool ExecuteCore()
    {
        if (File.Exists(ProjectAssetsFile) is false)
            return true;

        var projectAssetsText = File.ReadAllText(ProjectAssetsFile);
        var projectAssets = JObject.Parse(projectAssetsText);

        Log.LogMessage("Loading libraries");

        var libraries = projectAssets
            .Property("libraries", StringComparison.OrdinalIgnoreCase)
            ?.Value.Value<JObject>();

        if (libraries is null)
            return true;

        Log.LogMessage("Loading package folders");

        var packageFolders = projectAssets
            .Property("packageFolders", StringComparison.OrdinalIgnoreCase)
            ?.Value.Value<JObject>()
            ?.Properties()
            .Select(property => property.Name)
            .ToArray();

        if (packageFolders is null)
            return true;

        foreach (JProperty libraryProperty in libraries.Properties())
        {
            Log.LogMessage("Loading package library");
            var library = libraryProperty.Value.Value<JObject>();

            if (library is null)
                continue;

            Log.LogMessage("Loading package package type");

            var packageType = library
                .Property("type", StringComparison.OrdinalIgnoreCase)
                ?.Value.Value<string>();

            if (packageType is not "package")
                continue;

            Log.LogMessage("Loading package package path");

            var packagePath = library
                .Property("path", StringComparison.OrdinalIgnoreCase)
                ?.Value.Value<string>();

            if (packagePath is null)
                continue;

            Log.LogMessage("Loading files");
            var filesProperty = library.Property("files", StringComparison.OrdinalIgnoreCase);

            if (filesProperty is null)
                continue;

            var files = filesProperty.Value.Values<string>();

            foreach (string? file in files)
            {
                if (file?.StartsWith("schemas") is not true)
                    continue;

                var targetPath = Path.Combine(TargetDir, file);

                foreach (string packageFolder in packageFolders)
                {
                    var candidatePath = Path.Combine(packageFolder, packagePath, file);

                    if (File.Exists(candidatePath))
                    {
                        File.Copy(candidatePath, targetPath, overwrite: true);
                        break;
                    }
                }
            }
        }

        return true;
    }
}
