using Itmo.Dev.Platform.MessagePersistence.Internal.Models;
using Itmo.Dev.Platform.MessagePersistence.Internal.Tools;
using System.Diagnostics;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Extensions;

internal static class SerializedMessageExtensions
{
    internal static IEnumerable<ActivityLink> GetActivityLinks(this PersistedMessageModel message)
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
