using Itmo.Dev.Platform.Common.Exceptions;

namespace Itmo.Dev.Platform.YandexCloud.Exceptions;

public class YandexCloudException : PlatformException
{
    public YandexCloudException() { }

    public YandexCloudException(string? message) : base(message) { }
}