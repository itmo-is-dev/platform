using Itmo.Dev.Platform.Kafka.Producer;

namespace Itmo.Dev.Platform.Kafka.Tests.Outbox.Models;

public record KafkaOutboxConfigData(
    OutboxStrategy? OutboxStrategy,
    Type ExpectedProducerType,
    bool ShouldWriteOutboxMessages)
{
    public void ApplyConfig(Dictionary<string, string?> dictionary)
    {
        if (OutboxStrategy is null)
            return;

        dictionary["Producer:Outbox:Strategy"] = OutboxStrategy.ToString();
    }
}