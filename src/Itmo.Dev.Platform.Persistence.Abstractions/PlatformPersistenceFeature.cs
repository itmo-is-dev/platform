using Itmo.Dev.Platform.Common.Features;

namespace Itmo.Dev.Platform.Persistence.Abstractions;

internal class PlatformPersistenceFeature : IPlatformFeature
{
    public static string Name => "Persistence";
    public static string RegistrationMethod => "AddPlatformPersistence";
}