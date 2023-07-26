using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Testcontainers.Kafka;
using Xunit;

namespace Itmo.Dev.Platform.Kafka.Tests.Fixtures;

public class KafkaFixture : IAsyncLifetime
{
    private readonly HashSet<string> _createdTopics;
    private IAdminClient _adminClient = null!;

    public KafkaFixture()
    {
        Container = new KafkaBuilder()
            .WithImage("confluentinc/cp-kafka:latest")
            .WithPortBinding(50007, 9092)
            .Build();

        _createdTopics = new HashSet<string>();
    }

    public string Host => Container.GetBootstrapAddress();

    protected KafkaContainer Container { get; }

    public async Task CreateTopicsAsync(params string[] names)
    {
        var topicNames = names
            .Where(x => _createdTopics.Contains(x) is false)
            .ToArray();

        if (topicNames is [])
            return;

        foreach (string name in topicNames)
        {
            _createdTopics.Add(name);
        }

        var specifications = topicNames.Select(name => new TopicSpecification
        {
            Name = name,
        });

        await _adminClient.CreateTopicsAsync(specifications);
    }

    public async Task InitializeAsync()
    {
        await Container.StartAsync();

        var adminConfig = new AdminClientConfig
        {
            BootstrapServers = Host,
        };

        _adminClient = new AdminClientBuilder(adminConfig).Build();
    }

    public async Task DisposeAsync()
    {
        _adminClient.Dispose();
        await Container.DisposeAsync();
    }
}