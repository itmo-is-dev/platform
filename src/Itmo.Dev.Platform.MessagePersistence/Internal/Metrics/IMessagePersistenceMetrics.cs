namespace Itmo.Dev.Platform.MessagePersistence.Internal.Metrics;

internal interface IMessagePersistenceMetrics
{
    void IncMessageCreated(long count, string messageName);

    void IncMessageFinishedSuccessfully(string messageName);

    void IncMessageIgnored(string messageName);

    void IncMessageFailed(string messageName);
    
    void IncMessagePayloadMigrated(string messageName, PayloadVersion from, PayloadVersion to);
}
