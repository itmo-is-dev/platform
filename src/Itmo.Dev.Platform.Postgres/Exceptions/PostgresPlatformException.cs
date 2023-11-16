using Itmo.Dev.Platform.Common.Exceptions;

namespace Itmo.Dev.Platform.Postgres.Exceptions;

public class PostgresPlatformException : PlatformException
{
    internal PostgresPlatformException(string? message) : base(message) { }
}