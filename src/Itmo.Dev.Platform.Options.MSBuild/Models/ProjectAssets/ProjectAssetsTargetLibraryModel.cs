using System.Text.Json.Serialization;

namespace Itmo.Dev.Platform.Options.MSBuild.Models.ProjectAssets;

public sealed class ProjectAssetsTargetLibraryModel
{
    [JsonPropertyName("type")]
    public ProjectAssetsLibraryType Type { get; init; }

    [JsonPropertyName("compile")]
    public Dictionary<string, object> CompileReferences { get; init; } = [];

    [JsonPropertyName("runtime")]
    public Dictionary<string, object> RuntimeReferences { get; init; } = [];
}
