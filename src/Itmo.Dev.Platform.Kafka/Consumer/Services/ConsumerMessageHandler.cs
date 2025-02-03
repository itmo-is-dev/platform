using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.Kafka.Consumer.Models;
using Itmo.Dev.Platform.Kafka.Tools;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Threading.Channels;

namespace Itmo.Dev.Platform.Kafka.Consumer.Services;

internal class ConsumerMessageHandler<TKey, TValue>
{
    private readonly KafkaConsumerOptions _consumerOptions;
    private readonly IServiceScopeFactory _scopeFactory;

    public ConsumerMessageHandler(KafkaConsumerOptions consumerOptions, IServiceScopeFactory scopeFactory)
    {
        _consumerOptions = consumerOptions;
        _scopeFactory = scopeFactory;
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

            using var activity = PlatformKafkaActivitySource.Value.StartActivity(
                name: $"consume: {_consumerOptions.Topic}",
                ActivityKind.Consumer);

            await handler.HandleAsync(consumerMessages, cancellationToken);

            foreach (var consumerMessage in consumerMessages)
            {
                consumerMessage.Commit();
            }
        }
    }
}
