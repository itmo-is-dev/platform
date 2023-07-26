using Confluent.Kafka;
using Itmo.Dev.Platform.Kafka.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Kafka.Consumer.Services;

internal abstract class KafkaConsumerServiceBase<TKey, TValue> : BackgroundService
{
    private readonly IServiceResolver<IOptionsMonitor<IKafkaConsumerConfiguration>> _optionsResolver;
    private readonly IServiceResolver<IKafkaMessageHandler<TKey, TValue>> _handlerResolver;

    private readonly IServiceScopeFactory _scopeFactory;

    protected ILogger<KafkaConsumerServiceBase<TKey, TValue>> Logger { get; }
    protected IDeserializer<TKey> KeyDeserializer { get; }
    protected IDeserializer<TValue> ValueDeserializer { get; }

    protected KafkaConsumerServiceBase(
        IServiceResolver<IOptionsMonitor<IKafkaConsumerConfiguration>> optionsResolver,
        IServiceResolver<IKafkaMessageHandler<TKey, TValue>> handlerResolver,
        IServiceScopeFactory scopeFactory,
        ILogger<KafkaConsumerServiceBase<TKey, TValue>> logger,
        IDeserializer<TKey> keyDeserializer,
        IDeserializer<TValue> valueDeserializer)
    {
        _optionsResolver = optionsResolver;
        _scopeFactory = scopeFactory;
        Logger = logger;
        KeyDeserializer = keyDeserializer;
        ValueDeserializer = valueDeserializer;
        _handlerResolver = handlerResolver;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        while (stoppingToken.IsCancellationRequested is false)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var optionsMonitor = _optionsResolver.Resolve(scope.ServiceProvider);
            var options = optionsMonitor.CurrentValue;

            if (await CheckDisabledAsync(options, stoppingToken))
                continue;

            var handler = _handlerResolver.Resolve(scope.ServiceProvider);

            try
            {
                await ExecuteSingleAsync(options, handler, stoppingToken);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to consume messages");
            }
        }
    }

    protected abstract Task ExecuteSingleAsync(
        IKafkaConsumerConfiguration configuration,
        IKafkaMessageHandler<TKey, TValue> handler,
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