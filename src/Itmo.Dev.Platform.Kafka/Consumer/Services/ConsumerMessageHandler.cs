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
        foreach (KafkaConsumerMessage<TKey, TValue> message in messages)
        {
            var traceParentIndex = message.Headers.FindLastIndex(kvp => kvp.Key.Equals(
                PlatformKafkaConstants.Tracing.TraceParentHeader,
                StringComparison.OrdinalIgnoreCase));

            if (traceParentIndex < 0)
                continue;

            (_, string traceParentId) = message.Headers[traceParentIndex];

            if (ActivityContext.TryParse(traceParentId, traceState: null, out var context))
            {
                var tags = new ActivityTagsCollection();

                for (int i = message.Headers.Count - 1; i >= 0; i--)
                {
                    var (key, value) = message.Headers[i];

                    if (key == PlatformKafkaConstants.Tracing.TraceParentHeader)
                        continue;

                    tags.TryAdd(key, value);
                }

                yield return new ActivityLink(context, tags);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to parse activity context = '{TraceParent}' for message in topic '{TopicName}', partition = '{Partition}', offset = '{Offset}'",
                    traceParentId,
                    message.Topic,
                    message.Partition.Value,
                    message.Offset.Value);
            }
        }
    }
}
