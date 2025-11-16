using Confluent.Kafka;
using Itmo.Dev.Platform.Common.BackgroundServices;
using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.Common.Tools;
using Itmo.Dev.Platform.Kafka.Configuration;
using Itmo.Dev.Platform.Kafka.Consumer.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Channels;

namespace Itmo.Dev.Platform.Kafka.Consumer.Services;

internal sealed class BatchingKafkaConsumerService<TKey, TValue> : RestartableBackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly ILogger<BatchingKafkaConsumerService<TKey, TValue>> _logger;
    private readonly IDeserializer<TKey>? _keyDeserializer;
    private readonly IDeserializer<TValue>? _valueDeserializer;

    private readonly string _topicName;

    public BatchingKafkaConsumerService(
        string topicName,
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory,
        ILogger<BatchingKafkaConsumerService<TKey, TValue>> logger)
    {
        _topicName = topicName;
        _serviceProvider = serviceProvider;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _keyDeserializer = serviceProvider.GetKeyedService<IDeserializer<TKey>>(_topicName);
        _valueDeserializer = serviceProvider.GetKeyedService<IDeserializer<TValue>>(_topicName);
    }

    protected override async Task ExecuteAsync(CancellationTokenSource cts)
    {
        var consumerOptionsMonitor = _serviceProvider.GetRequiredService<IOptionsMonitor<KafkaConsumerOptions>>();
        var kafkaOptionsMonitor = _serviceProvider.GetRequiredService<IOptionsMonitor<PlatformKafkaOptions>>();

        using var _1 = consumerOptionsMonitor.OnNamedChange(_topicName, _ => cts.Cancel());
        using var _2 = kafkaOptionsMonitor.OnNamedChange(_topicName, _ => cts.Cancel());

        var consumerOptions = consumerOptionsMonitor.Get(_topicName);
        var kafkaOptions = kafkaOptionsMonitor.CurrentValue;

        if (consumerOptions.IsDisabled)
        {
            _logger.LogInformation(
                "Consumer for topic {Topic} is disabled",
                consumerOptions.Topic);

            await Task.Delay(-1, cts.Token);
        }

        while (cts.IsCancellationRequested is false)
        {
            try
            {
                await ExecuteSingleAsync(kafkaOptions, consumerOptions, cts.Token);
            }
            catch (Exception e) when
                (e is not OperationCanceledException or TaskCanceledException && cts.IsCancellationRequested is false)
            {
                _logger.LogError(e, "Failed to consume messages");
            }
        }
    }

    private async Task ExecuteSingleAsync(
        PlatformKafkaOptions kafkaOptions,
        KafkaConsumerOptions consumerOptions,
        CancellationToken cancellationToken)
    {
        var channelOptions = new BoundedChannelOptions(consumerOptions.BufferSize * 2)
        {
            SingleReader = consumerOptions.ParallelismDegree is 1,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.Wait,
        };

        var channel = Channel.CreateBounded<KafkaConsumerMessage<TKey, TValue>>(channelOptions);

        var messageReader = new ConsumerMessageReader<TKey, TValue>(
            kafkaOptions,
            consumerOptions,
            _keyDeserializer,
            _valueDeserializer);

        var messageHandler = new ConsumerMessageHandler<TKey, TValue>(
            consumerOptions,
            _scopeFactory,
            logger: _serviceProvider.GetRequiredService<ILogger<ConsumerMessageHandler<TKey, TValue>>>());

        await ParallelAction.ExecuteAsync(
            cancellationToken,
            new ParallelAction(1, c => messageReader.ReadAsync(channel.Writer, c)),
            new ParallelAction(consumerOptions.ParallelismDegree, c => messageHandler.HandleAsync(channel.Reader, c)));
    }
}
