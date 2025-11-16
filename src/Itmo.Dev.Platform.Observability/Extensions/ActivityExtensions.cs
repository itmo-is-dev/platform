using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Itmo.Dev.Platform.Observability.Extensions;

internal static class ActivityExtensions
{
    public static void Suppress(this Activity activity)
    {
        activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
    }

    public static bool TryGetTag(this Activity activity, string key, [NotNullWhen(true)] out string? value)
    {
        value = activity.Tags.FirstOrDefault(kvp => kvp.Key == key).Value;
        return value is not null;
    }
}
