using Itmo.Dev.Platform.MessagePersistence.Models;
using Itmo.Dev.Platform.MessagePersistence.Tools;
using System.Diagnostics;

namespace Itmo.Dev.Platform.MessagePersistence.Exceptions;

internal static class SerializedMessageExtensions
{
    public static IEnumerable<ActivityLink> GetActivityLinks(this SerializedMessage message)
    {
        var tags = new ActivityTagsCollection
        {
            [MessagePersistenceConstants.Tracing.MessageIdTag] = message.Id,
            [MessagePersistenceConstants.Tracing.MessageNameTag] = message.Name,
            [MessagePersistenceConstants.Tracing.MessageBufferingStepTag] = message.BufferingStep,
            [MessagePersistenceConstants.Tracing.MessageStateTag] = message.State,
            [MessagePersistenceConstants.Tracing.MessageRetryCountTag] = message.RetryCount,
        };

        var traceHeaders = message.Headers.Where(header => header.Key.Equals(
            MessagePersistenceConstants.Tracing.TraceParentHeader,
            StringComparison.OrdinalIgnoreCase));

        foreach (KeyValuePair<string, string> header in traceHeaders)
        {
            if (ActivityContext.TryParse(header.Value, null, out var context))
            {
                yield return new ActivityLink(context, tags);
            }
        }
    }
}
