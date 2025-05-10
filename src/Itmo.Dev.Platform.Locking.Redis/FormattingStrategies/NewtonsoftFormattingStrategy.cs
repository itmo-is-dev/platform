using Newtonsoft.Json;

namespace Itmo.Dev.Platform.Locking.Redis.FormattingStrategies;

internal class NewtonsoftFormattingStrategy : IKeyFormattingStrategy
{
    private readonly JsonSerializerSettings _serializerSettings;

    public NewtonsoftFormattingStrategy(JsonSerializerSettings serializerSettings)
    {
        _serializerSettings = serializerSettings;
    }

    public string Format(object key)
    {
        return JsonConvert.SerializeObject(key, Formatting.None, _serializerSettings);
    }
}
