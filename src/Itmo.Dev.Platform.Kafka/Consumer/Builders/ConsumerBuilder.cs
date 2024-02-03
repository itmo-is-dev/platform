using Confluent.Kafka;
using Itmo.Dev.Platform.Common.Models;
using Itmo.Dev.Platform.Kafka.Consumer.Inbox;
using Itmo.Dev.Platform.Kafka.Consumer.Models;
using Itmo.Dev.Platform.Kafka.Consumer.Services;
using Itmo.Dev.Platform.MessagePersistence.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Kafka.Consumer.Builders;

internal class ConsumerKeySelector : IConsumerKeySelector
{
    private readonly IServiceCollection _collection;

    public ConsumerKeySelector(IServiceCollection collection)
    {
        _collection = collection;
    }

    public IConsumerValueSelector<TKey> WithKey<TKey>()
        => new ConsumerValueSelector<TKey>(_collection);
}

internal class ConsumerValueSelector<TKey> : IConsumerValueSelector<TKey>
{
    private readonly IServiceCollection _collection;

    public ConsumerValueSelector(IServiceCollection collection)
    {
        _collection = collection;
    }

    public IConsumerConfigurationSelector<TKey, TValue> WithValue<TValue>()
        => new ConsumerConfigurationSelector<TKey, TValue>(_collection);
}

internal class ConsumerConfigurationSelector<TKey, TValue> : IConsumerConfigurationSelector<TKey, TValue>
{
    private readonly IServiceCollection _collection;

    public ConsumerConfigurationSelector(IServiceCollection collection)
    {
        _collection = collection;
    }

    public IConsumerKeyDeserializerSelector<TKey, TValue> WithConfiguration(
        IConfiguration configuration,
        Action<KafkaConsumerOptions>? action = null)
    {
        var topicSection = configuration.GetSection("Topic");
        var topicName = topicSection.Value;

        if (string.IsNullOrEmpty(topicName))
            throw new InvalidOperationException("Topic name is not specified");

        var builder = _collection
            .AddOptions<KafkaConsumerOptions>(topicName)
            .Bind(configuration)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        if (action is not null)
            builder.Configure(action);

        return new ConsumerBuilder<TKey, TValue>(_collection, topicName, configuration);
    }
}

internal class ConsumerBuilder<TKey, TValue> :
    IConsumerHandlerSelector<TKey, TValue>,
    IConsumerKeyDeserializerSelector<TKey, TValue>,
    IConsumerValueDeserializerSelector<TKey, TValue>,
    IConsumerBuilder
{
    private readonly IServiceCollection _collection;
    private readonly string _topicName;
    private readonly IConfiguration _configuration;

    public ConsumerBuilder(IServiceCollection collection, string topicName, IConfiguration configuration)
    {
        _collection = collection;
        _topicName = topicName;
        _configuration = configuration;
    }

    public IConsumerValueDeserializerSelector<TKey, TValue> DeserializeKeyWith<T>() where T : class, IDeserializer<TKey>
    {
        _collection.AddKeyedSingleton<IDeserializer<TKey>, T>(_topicName);
        return this;
    }

    public IConsumerValueDeserializerSelector<TKey, TValue> DeserializeKeyWith(IDeserializer<TKey> deserializer)
    {
        _collection.AddKeyedSingleton(_topicName, deserializer);
        return this;
    }

    public IConsumerValueDeserializerSelector<TKey, TValue> DeserializeKeyByDefault()
        => this;

    public IConsumerHandlerSelector<TKey, TValue> DeserializeValueWith<T>() where T : class, IDeserializer<TValue>
    {
        _collection.AddKeyedSingleton<IDeserializer<TValue>, T>(_topicName);
        return this;
    }

    public IConsumerHandlerSelector<TKey, TValue> DeserializeValueWith(IDeserializer<TValue> deserializer)
    {
        _collection.AddKeyedSingleton(_topicName, deserializer);
        return this;
    }

    public IConsumerHandlerSelector<TKey, TValue> DeserializeValueByDefault()
        => this;

    public IConsumerBuilder HandleWith<T>()
        where T : class, IKafkaConsumerHandler<TKey, TValue>
    {
        _collection.AddKeyedScoped<IKafkaConsumerHandler<TKey, TValue>, T>(_topicName);
        return this;
    }

    public IConsumerBuilder HandleInboxWith<T>() where T : class, IKafkaInboxHandler<TKey, TValue>
    {
        if (_configuration.GetSection("Inbox").Exists() is false)
        {
            string message = $"Inbox for topic {_topicName} is configured, but Inbox sub-section is not specified";
            throw new InvalidOperationException(message);
        }

        var messageName = $"_platform_kafka_inbox_{_topicName}";

        _collection.AddKeyedScoped<IKafkaConsumerHandler<TKey, TValue>>(
            _topicName,
            (p, _) => ActivatorUtilities.CreateInstance<InboxConsumerHandler<TKey, TValue>>(p, messageName));

        _collection.AddKeyedScoped<IKafkaInboxHandler<TKey, TValue>, T>(_topicName);

        _collection.AddPlatformMessagePersistenceHandler(builder => builder
            .Called(messageName)
            .WithConfiguration(_configuration.GetSection("Inbox"))
            .WithKey<Unit>()
            .WithValue<KafkaConsumerMessage<TKey, TValue>>()
            .HandleBy<InboxMessagePersistenceHandler<TKey, TValue>>((p, _) =>
                ActivatorUtilities.CreateInstance<InboxMessagePersistenceHandler<TKey, TValue>>(p, _topicName)));

        return this;
    }

    public void Build()
    {
        _collection.AddHostedService(p
            => ActivatorUtilities.CreateInstance<BatchingKafkaConsumerService<TKey, TValue>>(p, _topicName));
    }
}