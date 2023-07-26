using Confluent.Kafka;
using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.Kafka.Consumer.Models;
using Itmo.Dev.Platform.Kafka.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace Itmo.Dev.Platform.Kafka.Consumer.Services;

/// <summary>
///     Implementation optimized for batching (incomplete, not tested)
///     Waiting for the better times
/// </summary>
internal sealed class BatchingKafkaConsumerService<TKey, TValue> : KafkaConsumerServiceBase<TKey, TValue>
{
    public BatchingKafkaConsumerService(
        KeyValueQualifiedService<TKey, TValue, IOptionsMonitor<IKafkaConsumerConfiguration>> optionsResolver,
        KeyValueQualifiedService<TKey, TValue, IKafkaMessageHandler<TKey, TValue>> handlerResolver,
        IServiceScopeFactory scopeFactory,
        ILogger<BatchingKafkaConsumerService<TKey, TValue>> logger,
        IDeserializer<TKey> keyDeserializer,
        IDeserializer<TValue> valueDeserializer)
        : base(
            optionsResolver,
            handlerResolver,
            scopeFactory,
            logger,
            keyDeserializer,
            valueDeserializer) { }

    protected override async Task ExecuteSingleAsync(
        IKafkaConsumerConfiguration configuration,
        IKafkaMessageHandler<TKey, TValue> handler,
        CancellationToken cancellationToken)
    {
        var enumerable = EnumerateMessagesAsync(configuration, cancellationToken)
            .ChunkAsync(configuration.BufferSize, configuration.BufferWaitLimit, cancellationToken)
            .WithCancellation(cancellationToken);

        await foreach (var chunk in enumerable)
        {
            var consumerMessages = await chunk
                .Where(x => x.Message.Value is not null)
                .ToArrayAsync(cancellationToken);

            var messages = consumerMessages.Select(x => x.Message);
            await handler.HandleAsync(messages, cancellationToken);

            foreach (var consumerMessage in consumerMessages)
            {
                consumerMessage.Commit();
            }
        }
    }

    private async IAsyncEnumerable<InternalConsumerMessage<TKey, TValue>> EnumerateMessagesAsync(
        IKafkaConsumerConfiguration configuration,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var consumerConfiguration = new ConsumerConfig
        {
            GroupId = configuration.Group,
            BootstrapServers = configuration.Host,
        };

        using IConsumer<TKey, TValue> consumer = new ConsumerBuilder<TKey, TValue>(consumerConfiguration)
            .SetKeyDeserializer(KeyDeserializer)
            .SetValueDeserializer(ValueDeserializer)
            .Build();

        consumer.Subscribe(configuration.Topic);

        while (cancellationToken.IsCancellationRequested is false)
        {
            ConsumeResult<TKey, TValue> result = consumer.Consume(cancellationToken);
            yield return new InternalConsumerMessage<TKey, TValue>(result, consumer);
        }
    }
}