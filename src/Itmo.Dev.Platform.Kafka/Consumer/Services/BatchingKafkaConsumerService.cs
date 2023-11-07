using Confluent.Kafka;
using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.Kafka.Consumer.Models;
using Itmo.Dev.Platform.Kafka.QualifiedServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Itmo.Dev.Platform.Kafka.Consumer.Services;

internal sealed class BatchingKafkaConsumerService<TKey, TValue> : KafkaConsumerServiceBase<TKey, TValue>
{
    public BatchingKafkaConsumerService(
        IKeyValueQualifiedService<TKey, TValue, IKafkaConsumerConfiguration> optionsResolver,
        IKeyValueQualifiedService<TKey, TValue, IKafkaMessageHandler<TKey, TValue>> handlerResolver,
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory,
        ILogger<BatchingKafkaConsumerService<TKey, TValue>> logger,
        IDeserializer<TKey> keyDeserializer,
        IDeserializer<TValue> valueDeserializer)
        : base(
            optionsResolver,
            handlerResolver,
            serviceProvider,
            scopeFactory,
            logger,
            keyDeserializer,
            valueDeserializer) { }

    protected override async Task ExecuteSingleAsync(
        IKafkaConsumerConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        CancellationToken cancellationToken)
    {
        var channelOptions = new BoundedChannelOptions(configuration.BufferSize * 2)
        {
            SingleReader = configuration.ParallelismDegree is 1,
            SingleWriter = true,
        };

        var channel = Channel.CreateBounded<InternalConsumerMessage<TKey, TValue>>(channelOptions);

        var writeTask = WriteMessagesAsync(configuration, channel.Writer, cancellationToken);

        var handleTasks = Enumerable
            .Range(0, configuration.ParallelismDegree)
            .Select(async _ =>
            {
                await using var scope = scopeFactory.CreateAsyncScope();

                var options = OptionsResolver.Resolve(scope.ServiceProvider);
                var handler = HandlerResolver.Resolve(scope.ServiceProvider);

                await HandleMessagesAsync(options, channel.Reader, handler, cancellationToken);
            });

        var tasks = handleTasks.Prepend(writeTask);

        await Task.WhenAll(tasks);
    }

    private async Task WriteMessagesAsync(
        IKafkaConsumerConfiguration configuration,
        ChannelWriter<InternalConsumerMessage<TKey, TValue>> writer,
        CancellationToken cancellationToken)
    {
        await Task.Yield();

        var consumerConfiguration = new ConsumerConfig
        {
            GroupInstanceId = configuration.InstanceId,
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

        while (cancellationToken.IsCancellationRequested is false)
        {
            ConsumeResult<TKey, TValue> result = consumer.Consume(cancellationToken);
            var message = new InternalConsumerMessage<TKey, TValue>(result, consumer);

            await writer.WriteAsync(message, cancellationToken);
        }
    }

    private static async Task HandleMessagesAsync(
        IKafkaConsumerConfiguration configuration,
        ChannelReader<InternalConsumerMessage<TKey, TValue>> reader,
        IKafkaMessageHandler<TKey, TValue> handler,
        CancellationToken cancellationToken)
    {
        await Task.Yield();

        var enumerable = reader
            .ReadAllAsync(cancellationToken)
            .ChunkAsync(configuration.BufferSize, configuration.BufferWaitLimit);

        await foreach (var chunk in enumerable)
        {
            var consumerMessages = chunk
                .Where(x => x.Message.Value is not null)
                .ToArray();

            var messages = consumerMessages.Select(x => x.Message);
            await handler.HandleAsync(messages, cancellationToken);

            foreach (var consumerMessage in consumerMessages)
            {
                consumerMessage.Commit();
            }
        }
    }
}