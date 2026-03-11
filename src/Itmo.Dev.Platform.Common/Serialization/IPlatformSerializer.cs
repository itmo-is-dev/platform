namespace Itmo.Dev.Platform.Common.Serialization;

public interface IPlatformSerializer
{
    string Serialize<T>(T value);

    string Serialize<T>(T value, Type type);

    T? Deserialize<T>(string value);

    T? Deserialize<T>(string value, Type type)
        where T : class;
}
