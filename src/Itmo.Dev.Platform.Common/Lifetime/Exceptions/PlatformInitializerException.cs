using Itmo.Dev.Platform.Common.Exceptions;

namespace Itmo.Dev.Platform.Common.Lifetime.Exceptions;

public class PlatformInitializerException : PlatformException
{
    private PlatformInitializerException(string message) : base(message) { }

    internal static PlatformInitializerException InitializerCalledMultipleTimes(Type initializerType)
    {
        return new PlatformInitializerException(
            $"Initializer {initializerType.Name} is already running");
    }
}