using System.Diagnostics;

namespace Itmo.Dev.Platform.Common.Extensions;

public static class ActivityExtensions
{
    public static Activity? WithDisplayName(this Activity? activity, string displayName)
    {
        if (activity is not null)
        {
            activity.DisplayName = displayName;
        }

        return activity;
    }
}
