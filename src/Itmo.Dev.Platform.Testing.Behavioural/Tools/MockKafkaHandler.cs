using Itmo.Dev.Platform.Kafka.Consumer;
using Itmo.Dev.Platform.Testing.Behavioural.Tools.Converters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Concurrent;

namespace Itmo.Dev.Platform.Testing.Behavioural.Tools;

public sealed class MockKafkaHandler<TKey, TValue> : IKafkaConsumerHandler<TKey, TValue>
{
    private readonly ConcurrentBag<MockKafkaMessage<TKey, TValue>> _messages;
    private readonly ILogger<MockKafkaHandler<TKey, TValue>> _logger;
    private readonly IOptions<JsonSerializerSettings> _serializerSettings;

    public MockKafkaHandler(
        ConcurrentBag<MockKafkaMessage<TKey, TValue>> messages,
        ILogger<MockKafkaHandler<TKey, TValue>> logger,
        IOptions<JsonSerializerSettings> serializerSettings)
    {
        _messages = messages;
        _logger = logger;
        _serializerSettings = serializerSettings;
    }

    public ValueTask HandleAsync(
        IEnumerable<IKafkaConsumerMessage<TKey, TValue>> messages,
        CancellationToken cancellationToken)
    {
        MockKafkaMessage<TKey, TValue>[] mockMessages = messages
            .Select(message => new MockKafkaMessage<TKey, TValue>(message.Key, message.Value))
            .ToArray();

        var serializerSettings = new JsonSerializerSettings(_serializerSettings.Value);
        serializerSettings.Converters.Add(new TimestampConverter());
        serializerSettings.Converters.Add(new StringEnumConverter());

        _logger.LogInformation(
            "Received {Count} '{MessageName}' messages, values = {Values}",
            mockMessages.Length,
            typeof(TValue).Name,
            JsonConvert.SerializeObject(mockMessages, serializerSettings));

        foreach (MockKafkaMessage<TKey, TValue> message in mockMessages)
        {
            _messages.Add(message);
        }

        return ValueTask.CompletedTask;
    }
}
