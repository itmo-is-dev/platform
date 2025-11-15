using Itmo.Dev.Platform.Observability.Extensions;
using OpenTelemetry;
using System.Diagnostics;

namespace Itmo.Dev.Platform.Observability.Tracing.Processors;

internal sealed class MetricsActivityFilter : BaseProcessor<Activity>
{
    public override void OnEnd(Activity data)
    {
        if (data.Recorded is false)
            return;

        if (data.TryGetTag("url.scheme", out var scheme) is false || scheme is not "http")
            return;

        if (data.TryGetTag("url.path", out var path) is false || path is not "/metrics")
            return;

        data.Suppress();
    }
}
