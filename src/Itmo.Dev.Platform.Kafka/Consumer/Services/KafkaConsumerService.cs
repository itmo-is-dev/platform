using Confluent.Kafka;
using Itmo.Dev.Platform.Kafka.Consumer.Models;
using Itmo.Dev.Platform.Kafka.QualifiedServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Itmo.Dev.Platform.Kafka.Consumer.Services;

internal class KafkaConsumerService<TKey, TValue> : KafkaConsumerServiceBase<TKey, TValue>
{
    public KafkaConsumerService(
        IKeyValueQualifiedService<TKey, TValue, IKafkaConsumerConfiguration> optionsResolver,
        IKeyValueQualifiedService<TKey, TValue, IKafkaMessageHandler<TKey, TValue>> handlerResolver,
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory,
        ILogger<KafkaConsumerService<TKey, TValue>> logger,
        IDeserializer<TKey>? keyDeserializer = null,
        IDeserializer<TValue>? valueDeserializer = null)
        : base(
            optionsResolver,
            handlerResolver,
            serviceProvider,
            scopeFactory,
            logger,
            keyDeserializer,
            valueDeserializer) { }

    protected override async Task ExecuteSingleAsync(
        IKafkaConsumerConfiguration _,
        IServiceScopeFactory scopeFactory,
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var configuration = OptionsResolver.Resolve(scope.ServiceProvider);
        
        var consumerConfiguration = new ConsumerConfig
        {
            GroupId = configuration.Group,
            BootstrapServers = configuration.Host,
            AutoOffsetReset = configuration.ReadLatest ? AutoOffsetReset.Latest : AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            SecurityProtocol = configuration.SecurityProtocol,
        };

        using IConsumer<TKey, TValue> consumer = new ConsumerBuilder<TKey, TValue>(consumerConfiguration)
            .SetKeyDeserializer(KeyDeserializer)
            .SetValueDeserializer(ValueDeserializer)
            .Build();

        consumer.Subscribe(configuration.Topic);

        var handler = HandlerResolver.Resolve(scope.ServiceProvider);

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