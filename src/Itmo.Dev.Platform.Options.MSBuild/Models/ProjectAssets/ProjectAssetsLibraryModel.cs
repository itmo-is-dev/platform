using System.Text.Json.Serialization;

namespace Itmo.Dev.Platform.Options.MSBuild.Models.ProjectAssets;

public sealed class ProjectAssetsLibraryModel
{
    [JsonPropertyName("type")]
    public ProjectAssetsLibraryType Type { get; init; }

    [JsonPropertyName("path")]
    public string Path { get; init; } = string.Empty;

    [JsonPropertyName("files")]
    public string[] Files { get; init; } = [];
}
