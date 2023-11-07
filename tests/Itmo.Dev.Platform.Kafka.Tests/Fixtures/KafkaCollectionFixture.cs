using Xunit;

namespace Itmo.Dev.Platform.Kafka.Tests.Fixtures;

[CollectionDefinition(nameof(KafkaCollectionFixture))]
public class KafkaCollectionFixture : ICollectionFixture<KafkaFixture> { }