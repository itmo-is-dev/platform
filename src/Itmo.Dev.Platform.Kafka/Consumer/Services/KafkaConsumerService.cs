using Confluent.Kafka;
using Itmo.Dev.Platform.Kafka.Consumer.Models;
using Itmo.Dev.Platform.Kafka.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Kafka.Consumer.Services;

internal class KafkaConsumerService<TKey, TValue> : KafkaConsumerServiceBase<TKey, TValue>
{
    public KafkaConsumerService(
        KeyValueQualifiedService<TKey, TValue, IOptionsMonitor<IKafkaConsumerConfiguration>> optionsResolver,
        KeyValueQualifiedService<TKey, TValue, IKafkaMessageHandler<TKey, TValue>> handlerResolver,
        IServiceScopeFactory scopeFactory,
        ILogger<KafkaConsumerService<TKey, TValue>> logger,
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
        var consumerConfiguration = new ConsumerConfig
        {
            GroupId = configuration.Group,
            BootstrapServers = configuration.Host,
            AutoOffsetReset = configuration.ReadLatest ? AutoOffsetReset.Latest : AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        };

        using IConsumer<TKey, TValue> consumer = new ConsumerBuilder<TKey, TValue>(consumerConfiguration)
            .SetKeyDeserializer(KeyDeserializer)
            .SetValueDeserializer(ValueDeserializer)
            .Build();

        consumer.Subscribe(configuration.Topic);

        while (cancellationToken.IsCancellationRequested is false)
        {
            ConsumeResult<TKey, TValue> result = consumer.Consume(cancellationToken);
            var consumerMessage = new InternalConsumerMessage<TKey, TValue>(result, consumer);

            try
            {
                await handler.HandleAsync(new[] { consumerMessage.Message }, cancellationToken);
                consumer.Commit(result);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to handle message = {Message}", consumerMessage.Message);
            }
        }
    }
}