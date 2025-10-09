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
        Container = new KafkaBuilder().Build();
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

    public async Task ClearTopicsAsync(params string[] names)
    {
        var topics = await _adminClient.DescribeTopicsAsync(
            TopicCollection.OfTopicNames(names));

        var topicPartitionsOffsetSpecs = topics.TopicDescriptions
            .SelectMany(topic => topic.Partitions, (topic, topicPartition) => (topic, topicPartition))
            .Select(tuple => new TopicPartitionOffsetSpec
            {
                TopicPartition = new TopicPartition(tuple.topic.Name, tuple.topicPartition.Partition),
                OffsetSpec = OffsetSpec.Latest(),
            });

        var offsets = await _adminClient.ListOffsetsAsync(topicPartitionsOffsetSpecs);

        await _adminClient.DeleteRecordsAsync(
            offsets.ResultInfos.Select(x => x.TopicPartitionOffsetError.TopicPartitionOffset));
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
