using Newtonsoft.Json;

namespace Itmo.Dev.Platform.Locking.FormattingStrategies;

internal class NewtonsoftLockingKeyFormatter : ILockingKeyFormatter
{
    private readonly JsonSerializerSettings _serializerSettings;

    public NewtonsoftLockingKeyFormatter(JsonSerializerSettings serializerSettings)
    {
        _serializerSettings = serializerSettings;
    }

    public string Format(object key)
    {
        return JsonConvert.SerializeObject(key, Formatting.None, _serializerSettings);
    }
}
