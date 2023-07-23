namespace Itmo.Dev.Platform.Common.Exceptions;

public class PlatformException : Exception
{
    public PlatformException() { }
    public PlatformException(string? message) : base(message) { }
    public PlatformException(string? message, Exception? innerException) : base(message, innerException) { }
}