using Itmo.Dev.Platform.MessagePersistence.Internal.Tools;
using System.Diagnostics.Metrics;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Metrics;

internal sealed class MessagePersistenceMetrics : IMessagePersistenceMetrics
{
    private const string MessageNameKey = "message_name";

    private readonly Counter<long> _messageCreatedCounter = MessagePersistenceActivitySource.Meter
        .CreateCounter<long>(
            name: "itmo_dev_platform_message_persistence_created_total",
            unit: "message",
            description: "Total count of created messages");

    private readonly Counter<long> _messageFinishedSuccessfullyCounter = MessagePersistenceActivitySource.Meter
        .CreateCounter<long>(
            name: "itmo_dev_platform_message_persistence_finished_total",
            unit: "message",
            description: "Total count of messages finished successfully");

    private readonly Counter<long> _messageIgnoredCounter = MessagePersistenceActivitySource.Meter
        .CreateCounter<long>(
            name: "itmo_dev_platform_message_persistence_ignored_total",
            unit: "message",
            description: "Total count of ignored messages");

    private readonly Counter<long> _messageFailedCounter = MessagePersistenceActivitySource.Meter
        .CreateCounter<long>(
            name: "itmo_dev_platform_message_persistence_failed",
            unit: "message",
            description: "Total count of failed messages");

    private readonly Counter<long> _messagePayloadMigratedCounter = MessagePersistenceActivitySource.Meter
        .CreateCounter<long>(
            name: "itmo_dev_platform_message_persistence_migrated_messages_total",
            unit: "message",
            description: "Total count of performed message payload migrations ");

    public void IncMessageCreated(long count, string messageName)
        => _messageCreatedCounter.Add(
            delta: count,
            new KeyValuePair<string, object?>(MessageNameKey, messageName));

    public void IncMessageFinishedSuccessfully(string messageName)
        => _messageFinishedSuccessfullyCounter.Add(
            delta: 1,
            new KeyValuePair<string, object?>(MessageNameKey, messageName));

    public void IncMessageIgnored(string messageName)
        => _messageIgnoredCounter.Add(
            delta: 1,
            new KeyValuePair<string, object?>(MessageNameKey, messageName));

    public void IncMessageFailed(string messageName)
        => _messageFailedCounter.Add(
            delta: 1,
            new KeyValuePair<string, object?>(MessageNameKey, messageName));

    public void IncMessagePayloadMigrated(string messageName, PayloadVersion from, PayloadVersion to)
        => _messagePayloadMigratedCounter.Add(
            delta: 1,
            new(MessageNameKey, messageName),
            new("version_from", from.Value),
            new("version_to", to.Value));
}
