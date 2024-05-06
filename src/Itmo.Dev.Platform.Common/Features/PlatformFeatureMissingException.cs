using Itmo.Dev.Platform.Common.Exceptions;

namespace Itmo.Dev.Platform.Common.Features;

public class PlatformFeatureMissingException : PlatformException
{
    private PlatformFeatureMissingException(string message) : base(message) { }

    internal static PlatformFeatureMissingException Create<T>()
        where T : IPlatformFeature
    {
        return new PlatformFeatureMissingException(
            $"Platform's {T.Name} was not registered. Please call {T.RegistrationMethod} on service collection");
    }
}