using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Itmo.Dev.Platform.Common.Serialization;

internal sealed class SystemTextJsonSerializer : IPlatformSerializer
{
    private readonly IOptions<JsonSerializerOptions> _options;

    public SystemTextJsonSerializer(IOptions<JsonSerializerOptions> options)
    {
        _options = options;
    }

    public string Serialize<T>(T value)
        => JsonSerializer.Serialize(value, _options.Value);

    public string Serialize<T>(T value, Type type)
        => JsonSerializer.Serialize(value, type, _options.Value);

    public T? Deserialize<T>(string value)
        => JsonSerializer.Deserialize<T>(value, _options.Value);

    public T? Deserialize<T>(string value, Type type)
        where T : class
    {
        return JsonSerializer.Deserialize(value, type, _options.Value) as T;
    }
}
