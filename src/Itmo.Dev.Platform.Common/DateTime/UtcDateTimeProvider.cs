namespace Itmo.Dev.Platform.Common.DateTime;

public class UtcDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset Current => DateTimeOffset.UtcNow;
}