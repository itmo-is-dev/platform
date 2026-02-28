using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using Testcontainers.Kafka;
using Xunit;

namespace Itmo.Dev.Platform.Testing.Fixtures;

public sealed class KafkaFixture : IAsyncLifetime
{
    private readonly TopicSpecification[] _topics;

    public KafkaFixture(params TopicSpecification[] topics)
    {
        _topics = topics;

        Container = new KafkaBuilder("confluentinc/cp-kafka:7.5.9")
            .Build();
    }

    public string Host => Container.GetBootstrapAddress();

    public KafkaContainer Container { get; }

    public void ConfigureAppConfiguration(IConfigurationBuilder builder)
    {
        builder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Presentation:Kafka:Host"] = Host,
        });
    }

    public async Task InitializeAsync()
    {
        await Container.StartAsync();

        var adminConfig = new AdminClientConfig
        {
            BootstrapServers = Host,
        };

        IAdminClient adminClient = new AdminClientBuilder(adminConfig).Build();
        await adminClient.CreateTopicsAsync(_topics);
    }

    public async Task DisposeAsync()
    {
        await Container.DisposeAsync();
    }
}
