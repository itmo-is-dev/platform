using Itmo.Dev.Platform.Common.Features;

namespace Itmo.Dev.Platform.Enrichment.Tools;

internal class PlatformEnrichmentFeature : IPlatformFeature
{
    public static string Name => "Enrichment";

    public static string RegistrationMethod => nameof(ServiceCollectionExtensions.AddPlatformEnrichment);
}
