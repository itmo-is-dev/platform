using Itmo.Dev.Platform.Kafka.Producer;
using Microsoft.Extensions.Configuration;

namespace Itmo.Dev.Platform.Kafka.Tests.Outbox;

public record KafkaOutboxConfigData(
    OutboxStrategy? OutboxStrategy,
    Type ExpectedProducerType,
    bool ShouldWriteOutboxMessages)
{
    public void ApplyConfig(IConfigurationBuilder builder)
    {
        if (OutboxStrategy is null)
            return;

        builder.AddInMemoryCollection(
            new Dictionary<string, string?> { ["Producer:Outbox:Strategy"] = OutboxStrategy.ToString() });
    }
}
