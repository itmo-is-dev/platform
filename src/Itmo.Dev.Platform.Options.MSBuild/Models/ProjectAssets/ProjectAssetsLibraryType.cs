using System.Text.Json.Serialization;

namespace Itmo.Dev.Platform.Options.MSBuild.Models.ProjectAssets;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProjectAssetsLibraryType
{
    [JsonStringEnumMemberName("package")]
    Package = 1,

    [JsonStringEnumMemberName("project")]
    Project = 2,
}
