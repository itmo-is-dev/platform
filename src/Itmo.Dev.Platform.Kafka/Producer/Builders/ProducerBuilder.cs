using Confluent.Kafka;
using Itmo.Dev.Platform.Kafka.Producer.Outbox;
using Itmo.Dev.Platform.Kafka.Producer.Services;
using Itmo.Dev.Platform.MessagePersistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Itmo.Dev.Platform.Kafka.Producer.Builders;

internal class ProducerKeySelector : IProducerKeySelector
{
    private readonly IServiceCollection _collection;

    public ProducerKeySelector(IServiceCollection collection)
    {
        _collection = collection;
    }

    public IProducerValueSelector<TKey> WithKey<TKey>()
    {
        return new ProducerValueSelector<TKey>(_collection);
    }
}

internal class ProducerValueSelector<TKey> : IProducerValueSelector<TKey>
{
    private readonly IServiceCollection _collection;

    public ProducerValueSelector(IServiceCollection collection)
    {
        _collection = collection;
    }

    public IProducerConfigurationSelector<TKey, TValue> WithValue<TValue>()
    {
        return new ProducerConfigurationSelector<TKey, TValue>(_collection);
    }
}

internal class ProducerConfigurationSelector<TKey, TValue> : IProducerConfigurationSelector<TKey, TValue>
{
    private readonly IServiceCollection _collection;

    public ProducerConfigurationSelector(IServiceCollection collection)
    {
        _collection = collection;
    }

    public IProducerKeySerializerSelector<TKey, TValue> WithConfiguration(
        IConfiguration configuration,
        Action<KafkaProducerOptions>? action = null)
    {
        var topicSection = configuration.GetSection("Topic");
        var topicName = topicSection.Value;

        if (string.IsNullOrEmpty(topicName))
            throw new InvalidOperationException("Topic name is not specified");

        var builder = _collection
            .AddOptions<KafkaProducerOptions>(topicName)
            .Bind(configuration)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        if (action is not null)
            builder.Configure(action);

        return new ProducerBuilder<TKey, TValue>(topicName, _collection, configuration);
    }
}

internal class ProducerBuilder<TKey, TValue> :
    IProducerKeySerializerSelector<TKey, TValue>,
    IProducerValueSerializerSelector<TKey, TValue>,
    IOutboxProducerBuilder
{
    private readonly string _topicName;
    private readonly IServiceCollection _collection;
    private readonly IConfiguration _configuration;

    public ProducerBuilder(string topicName, IServiceCollection collection, IConfiguration configuration)
    {
        _topicName = topicName;
        _collection = collection;
        _configuration = configuration;
    }

    public IProducerValueSerializerSelector<TKey, TValue> SerializeKeyWith<T>()
        where T : class, ISerializer<TKey>
    {
        _collection.AddKeyedSingleton<ISerializer<TKey>, T>(_topicName);
        return this;
    }

    public IProducerValueSerializerSelector<TKey, TValue> SerializeKeyWith(ISerializer<TKey> serializer)
    {
        _collection.AddKeyedSingleton(_topicName, serializer);
        return this;
    }

    public IProducerValueSerializerSelector<TKey, TValue> SerializeByDefault()
        => this;

    public IOutboxProducerBuilder SerializeValueWith<T>()
        where T : class, ISerializer<TValue>
    {
        _collection.AddKeyedSingleton<ISerializer<TValue>, T>(_topicName);
        return this;
    }

    public IOutboxProducerBuilder SerializeValueWith(ISerializer<TValue> serializer)
    {
        _collection.AddKeyedSingleton(_topicName, serializer);
        return this;
    }

    public IOutboxProducerBuilder SerializeValueByDefault()
        => this;

    public IProducerBuilder WithOutbox()
    {
        IConfigurationSection outboxSection = _configuration.GetSection("Outbox");

        var outboxStrategy = outboxSection.GetValue("Strategy", OutboxStrategy.Always);

        if (outboxStrategy is OutboxStrategy.Fallback)
        {
            _collection.AddScoped<IKafkaMessageProducer<TKey, TValue>>(provider => ActivatorUtilities
                .CreateInstance<FallbackOutboxMessageProducer<TKey, TValue>>(provider, _topicName));
        }
        else
        {
            _collection.AddScoped<IKafkaMessageProducer<TKey, TValue>>(provider => ActivatorUtilities
                .CreateInstance<AlwaysOutboxMessageProducer<TKey, TValue>>(provider, _topicName));
        }

        _collection.AddPlatformMessagePersistenceHandler(builder => builder
            .Called(KafkaOutboxMessageName.ForTopic(_topicName))
            .WithConfiguration(outboxSection)
            .WithMessage<OutboxPersistedMessage<TKey, TValue>>()
            .HandleBy<OutboxPersistedMessageHandler<TKey, TValue>>((provider, _)
                => new OutboxPersistedMessageHandler<TKey, TValue>(_topicName, provider)));

        return this;
    }

    public void Build()
    {
        _collection.AddKeyedScoped<IKafkaMessageProducer<TKey, TValue>>(
            _topicName,
            (p, _) => ActivatorUtilities.CreateInstance<KafkaMessageProducer<TKey, TValue>>(p, _topicName));

        _collection.TryAddScoped<IKafkaMessageProducer<TKey, TValue>>(p
            => ActivatorUtilities.CreateInstance<KafkaMessageProducer<TKey, TValue>>(p, _topicName));
    }
}
