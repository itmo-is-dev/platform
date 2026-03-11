using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Itmo.Dev.Platform.Common.Serialization;

internal sealed class NewtonsoftSerializer : IPlatformSerializer
{
    private readonly IOptions<JsonSerializerSettings> _options;

    public NewtonsoftSerializer(IOptions<JsonSerializerSettings> options)
    {
        _options = options;
    }

    public string Serialize<T>(T value)
        => JsonConvert.SerializeObject(value, _options.Value);

    public string Serialize<T>(T value, Type type)
        => JsonConvert.SerializeObject(value, type, _options.Value);

    public T? Deserialize<T>(string value)
        => JsonConvert.DeserializeObject<T>(value, _options.Value);

    public T? Deserialize<T>(string value, Type type)
        where T : class
    {
        return JsonConvert.DeserializeObject(value, type, _options.Value) as T;
    }
}
