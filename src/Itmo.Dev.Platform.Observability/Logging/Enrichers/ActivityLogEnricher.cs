using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;

namespace Itmo.Dev.Platform.Observability.Logging.Enrichers;

internal class ActivityLogEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;

        if (activity is null)
            return;

        logEvent.AddOrUpdateProperty(new LogEventProperty("TraceId", new ScalarValue(activity.TraceId.ToHexString())));
        logEvent.AddOrUpdateProperty(new LogEventProperty("SpanId", new ScalarValue(activity.SpanId.ToHexString())));

        foreach (KeyValuePair<string, string?> tag in activity.Tags)
        {
            if (tag.Value is null)
                continue;

            logEvent.AddOrUpdateProperty(new LogEventProperty(tag.Key, new ScalarValue(tag.Value)));
        }
    }
}