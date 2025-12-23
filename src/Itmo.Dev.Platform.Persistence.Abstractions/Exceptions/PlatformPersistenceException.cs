using Itmo.Dev.Platform.Common.Exceptions;

namespace Itmo.Dev.Platform.Persistence.Abstractions.Exceptions;

public abstract class PlatformPersistenceException : PlatformException
{
    private protected PlatformPersistenceException() { }

    private protected PlatformPersistenceException(string? message)
        : base(message) { }

    private protected PlatformPersistenceException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
