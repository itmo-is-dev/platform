using System.Text.Json;
using System.Text.Json.Serialization;

namespace Itmo.Dev.Platform.Options.MSBuild.Models.ProjectAssets;

[JsonConverter(typeof(ProjectAssetsPackageNameConverter))]
public sealed record ProjectAssetsLibraryName(
    string Name,
    string? Version);

file class ProjectAssetsPackageNameConverter : JsonConverter<ProjectAssetsLibraryName>
{
    public override ProjectAssetsLibraryName? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var text = reader.GetString();

        if (text is null)
            return null;

        return Parse(text);
    }

    public override void Write(Utf8JsonWriter writer, ProjectAssetsLibraryName value, JsonSerializerOptions options)
    {
        writer.WriteStringValue($"{value.Name}/{value.Version}");
    }

    public override ProjectAssetsLibraryName ReadAsPropertyName(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var text = reader.GetString();

        if (text is null)
            throw new JsonException("Property name cannot be null");

        return Parse(text);
    }

    public override void WriteAsPropertyName(
        Utf8JsonWriter writer,
        ProjectAssetsLibraryName value,
        JsonSerializerOptions options)
    {
        writer.WritePropertyName($"{value.Name}/{value.Version}");
    }

    private ProjectAssetsLibraryName Parse(string text)
    {
        var parts = text.Split("/", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (parts is [var singlePackageName])
            return new ProjectAssetsLibraryName(singlePackageName, Version: null);

        if (parts is [var packageName, var packageVersion])
            return new ProjectAssetsLibraryName(packageName, packageVersion);

        throw new JsonException($"Invalid package name format = {text}");
    }
}
