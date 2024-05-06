using Itmo.Dev.Platform.Common.Exceptions;

namespace Itmo.Dev.Platform.Common.Features;

public class PlatformFeatureAlreadyRegisteredException : PlatformException
{
    private PlatformFeatureAlreadyRegisteredException(string message) : base(message) { }

    internal static PlatformFeatureAlreadyRegisteredException Create<T>()
        where T : IPlatformFeature
    {
        return new PlatformFeatureAlreadyRegisteredException(
            $"Platform's {T.Name} is already registered. Please check your configuration");
    }
}