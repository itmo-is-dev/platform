namespace Itmo.Dev.Platform.Common.Exceptions;

public class PlatformException : Exception
{
    private protected PlatformException() { }

    private protected PlatformException(string? message) : base(message) { }

    private protected PlatformException(string? message, Exception? innerException) : base(message, innerException) { }
}