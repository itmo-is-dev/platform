using Confluent.Kafka;
using Itmo.Dev.Platform.Kafka.Configuration;
using Itmo.Dev.Platform.Kafka.QualifiedServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Kafka.Consumer.Services;

internal abstract class KafkaConsumerServiceBase<TKey, TValue> : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScopeFactory _scopeFactory;

    protected ILogger<KafkaConsumerServiceBase<TKey, TValue>> Logger { get; }
    protected IDeserializer<TKey>? KeyDeserializer { get; }
    protected IDeserializer<TValue>? ValueDeserializer { get; }
    protected IServiceResolver<IKafkaMessageHandler<TKey, TValue>> HandlerResolver { get; }

    protected IServiceResolver<IKafkaConsumerConfiguration> OptionsResolver { get; }

    protected KafkaConsumerServiceBase(
        IServiceResolver<IKafkaConsumerConfiguration> optionsResolver,
        IServiceResolver<IKafkaMessageHandler<TKey, TValue>> handlerResolver,
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory,
        ILogger<KafkaConsumerServiceBase<TKey, TValue>> logger,
        IDeserializer<TKey>? keyDeserializer,
        IDeserializer<TValue>? valueDeserializer)
    {
        OptionsResolver = optionsResolver;
        _serviceProvider = serviceProvider;
        Logger = logger;
        KeyDeserializer = keyDeserializer;
        ValueDeserializer = valueDeserializer;
        _scopeFactory = scopeFactory;
        HandlerResolver = handlerResolver;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        while (stoppingToken.IsCancellationRequested is false)
        {
            var options = OptionsResolver.Resolve(_serviceProvider);

            if (await CheckDisabledAsync(options, stoppingToken))
                continue;

            var kafkaOptions = _serviceProvider.GetRequiredService<IOptionsMonitor<KafkaConfiguration>>();

            try
            {
                await ExecuteSingleAsync(kafkaOptions.CurrentValue, options, _scopeFactory, stoppingToken);
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                Logger.LogError(e, "Failed to consume messages");
            }
        }
    }

    protected abstract Task ExecuteSingleAsync(
        KafkaConfiguration kafkaConfiguration,
        IKafkaConsumerConfiguration configuration,
        IServiceScopeFactory factory,
        CancellationToken cancellationToken);

    private async Task<bool> CheckDisabledAsync(IKafkaConsumerConfiguration options, CancellationToken stoppingToken)
    {
        if (options.IsDisabled)
        {
            Logger.LogInformation(
                "Consumer for topic {Topic} is disabled, waiting for {Span}s",
                options.Topic,
                options.DisabledConsumerTimeout);

            await Task.Delay(options.DisabledConsumerTimeout, stoppingToken);
            return true;
        }

        return false;
    }
}