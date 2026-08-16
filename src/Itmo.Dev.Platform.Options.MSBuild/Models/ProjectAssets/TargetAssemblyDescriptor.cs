namespace Itmo.Dev.Platform.Options.MSBuild.Models.ProjectAssets;

public sealed record TargetAssemblyDescriptor(
    ProjectAssetsLibraryName LibraryName,
    string PathInPacakge);
