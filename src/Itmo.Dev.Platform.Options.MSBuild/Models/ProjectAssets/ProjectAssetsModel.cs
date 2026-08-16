using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Itmo.Dev.Platform.Options.MSBuild.Models.ProjectAssets;

public sealed class ProjectAssetsModel
{
    [JsonPropertyName("packageFolders")]
    public Dictionary<string, PackageFolderModel> PackageFolders { get; init; } = [];

    [JsonPropertyName("libraries")]
    public Dictionary<ProjectAssetsLibraryName, ProjectAssetsLibraryModel> Libraries { get; init; } = [];

    [JsonPropertyName("targets")]
    public Dictionary<string, ProjectAssetsTargetModel> Targets { get; init; } = [];

    public bool TryGetAssemblyDescriptor(
        string targetFramework,
        AssemblyName assemblyName,
        [NotNullWhen(true)] out TargetAssemblyDescriptor? descriptor)
    {
        if (Targets.TryGetValue(targetFramework, out var target) is false)
        {
            descriptor = null;
            return false;
        }

        foreach (KeyValuePair<ProjectAssetsLibraryName, ProjectAssetsTargetLibraryModel> targetLibrary in target)
        {
            foreach (string referencePath in targetLibrary.Value.RuntimeReferences.Keys)
            {
                var referenceAssemblyName = Path.GetFileNameWithoutExtension(referencePath);

                if (referenceAssemblyName.Equals(assemblyName.Name, StringComparison.OrdinalIgnoreCase) is false)
                    continue;

                descriptor = new TargetAssemblyDescriptor(targetLibrary.Key, referencePath);
                return true;
            }
        }

        descriptor = null;
        return false;
    }

    public bool TryGetPackageDirectoryPath(string packagePath, [NotNullWhen(true)] out string? packageFullPath)
    {
        packageFullPath = null;

        foreach (string packageFolderPath in PackageFolders.Keys)
        {
            var candidatePath = Path.Combine(packageFolderPath, packagePath);

            if (Directory.Exists(candidatePath))
            {
                packageFullPath = candidatePath;
                return true;
            }
        }

        return false;
    }

    public static bool TryGetFromFile(string path, [NotNullWhen(true)] out ProjectAssetsModel? projectAssets)
    {
        if (File.Exists(path) is false)
        {
            projectAssets = null;
            return false;
        }

        var content = File.ReadAllText(path);
        var model = JsonSerializer.Deserialize<ProjectAssetsModel>(content);

        projectAssets = model ?? throw new InvalidOperationException("Failed to deserialize project assets file");
        return true;
    }

    public static ProjectAssetsModel FromFile(string path)
    {
        return TryGetFromFile(path, out var projectAssets) ? projectAssets : new ProjectAssetsModel();
    }

    public sealed record PackageFolderModel;
}
