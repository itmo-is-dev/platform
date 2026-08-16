using Itmo.Dev.Platform.Options.MSBuild.Models.ProjectAssets;
using Microsoft.Build.Framework;
using System.Reflection;
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
        catch (ReflectionTypeLoadException)
        {
            Log.LogWarning("Failed to process option type, try rebuilding the project");
        }
        catch (Exception e)
        {
            Log.LogError("Error while executing '{0}' = {1}", nameof(CopyReferencedSchemasTask), e);
        }

        return true;
    }

    private bool ExecuteCore()
    {
        if (ProjectAssetsModel.TryGetFromFile(ProjectAssetsFile, out var projectAssets) is false)
            return true;

        foreach (KeyValuePair<ProjectAssetsLibraryName, ProjectAssetsLibraryModel> library in projectAssets.Libraries)
        {
            if (library.Value.Type is not ProjectAssetsLibraryType.Package)
                continue;

            if (projectAssets.TryGetPackageDirectoryPath(library.Value.Path, out var packagePath) is false)
                continue;

            foreach (string file in library.Value.Files)
            {
                if (file.StartsWith("schemas") is false)
                    continue;

                var sourcePath = Path.Combine(packagePath, file);
                var targetPath = Path.Combine(TargetDir, file);

                if (File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, targetPath, overwrite: true);
                }
            }
        }

        return true;
    }
}
