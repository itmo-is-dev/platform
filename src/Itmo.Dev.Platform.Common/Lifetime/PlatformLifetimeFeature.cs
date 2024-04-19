using Itmo.Dev.Platform.Common.Features;
using ServiceCollectionExtensions = Itmo.Dev.Platform.Common.Extensions.ServiceCollectionExtensions;

namespace Itmo.Dev.Platform.Common.Lifetime;

internal class PlatformLifetimeFeature : IPlatformFeature
{
    public static string Name => "lifetimes";
    public static string RegistrationMethod => nameof(ServiceCollectionExtensions.AddPlatform);
}