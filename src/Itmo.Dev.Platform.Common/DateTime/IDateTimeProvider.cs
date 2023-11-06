namespace Itmo.Dev.Platform.Common.DateTime;

public interface IDateTimeProvider
{
    DateTimeOffset Current { get; }
}