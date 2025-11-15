using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.Kafka.Consumer.Models;
using Itmo.Dev.Platform.Kafka.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Channels;

namespace Itmo.Dev.Platform.Kafka.Consumer.Services;

internal class ConsumerMessageHandler<TKey, TValue>
{
    private readonly KafkaConsumerOptions _consumerOptions;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ConsumerMessageHandler<TKey, TValue>> _logger;

    public ConsumerMessageHandler(
        KafkaConsumerOptions consumerOptions,
        IServiceScopeFactory scopeFactory,
        ILogger<ConsumerMessageHandler<TKey, TValue>> logger)
    {
        _consumerOptions = consumerOptions;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task HandleAsync(
        ChannelReader<KafkaConsumerMessage<TKey, TValue>> reader,
        CancellationToken cancellationToken)
    {
        await Task.Yield();

        var enumerable = reader
            .ReadAllAsync(cancellationToken)
            .ChunkAsync(_consumerOptions.BufferSize, _consumerOptions.BufferWaitLimit);

        await foreach (var chunk in enumerable)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var handler = scope.ServiceProvider
                .GetRequiredKeyedService<IKafkaConsumerHandler<TKey, TValue>>(_consumerOptions.Topic);

            var consumerMessages = chunk
                .Where(x => x.Value is not null)
                .ToArray();

            using var activity = PlatformKafkaActivitySource.Value
                .StartActivity(
                    name: "Kafka",
                    ActivityKind.Consumer,
                    parentContext: default,
                    links: [..EnumerateActivityLinks(chunk)])
                .WithDisplayName($"[consume] {_consumerOptions.Topic}");

            await handler.HandleAsync(consumerMessages, cancellationToken);

            foreach (var consumerMessage in consumerMessages)
            {
                consumerMessage.Commit();
            }
        }
    }

    private IEnumerable<ActivityLink> EnumerateActivityLinks(
        IEnumerable<KafkaConsumerMessage<TKey, TValue>> messages)
    {
        var traceParentHeaders = messages
            .SelectMany(message => message.Headers, (message, header) => (message, header))
            .Where(tuple => tuple.header.Key.Equals(
                PlatformKafkaConstants.TraceParentHeaderName,
                StringComparison.OrdinalIgnoreCase));

        foreach (var (message, header) in traceParentHeaders)
        {
            if (ActivityContext.TryParse(header.Value, null, out var context))
            {
                var tags = new ActivityTagsCollection
                {
                    [PlatformKafkaConstants.TopicTagName] = message.Topic,
                    [PlatformKafkaConstants.PartitionTagName] = message.Partition.Value,
                    [PlatformKafkaConstants.OffsetTagName] = message.Offset.Value,
                };

                yield return new ActivityLink(context, tags);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to parse activity context = '{TraceParent}'",
                    header.Value);
            }
        }
    }
}
