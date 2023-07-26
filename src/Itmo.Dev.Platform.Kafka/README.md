# Itmo.Dev.Platform.Kafka

Platform extensions for working with Kafka

## Consumer

### Implement message handler

```csharp
public class MyMessageHandler : IKafkaMessageHandler<int, string>
{
    public ValueTask HandleAsync(
        IEnumerable<ConsumerKafkaMessage<TKey, TValue>> messages, 
        CancellationToken cancellationToken)
    {
        foreach (var message in messages)
        {
            Console.WriteLine($"Received message, Key = {message.Key}, Value = {message.Value}");
        }
        
        return ValueTask.CompletedTask;
    }
}
```

### Implement configuration type

```csharp
public class Configuration : IKafkaConsumerConfiguration
{
    public bool IsDisabled { get; init; }

    public TimeSpan DisabledConsumerTimeout { get; init; }

    public string Host { get; set; } = string.Empty;

    public string Topic { get; init; } = string.Empty;

    public string Group { get; init; } = string.Empty;

    public int ParallelismDegree { get; init; }

    public int BufferSize { get; init; }

    public TimeSpan BufferWaitLimit { get; init; }

    public bool ReadLatest { get; init; }
}
```

At runtime, configuration would be received as `IOptionsMonitor<TConfiguration>`, so you should manually register it
to DI container as options.

### Add consumer to your ASP.NET server services

> Consumer requires an ASP.NET host, as it is implemented as hosted service

Use extension methods to register your consumer.

```csharp
collection.AddKafkaConsumer<int, string>(builder => builder
    .HandleWith<MyMessageHandler>()
    .DeserializeKeyWithNewtonsoft()
    .DeserializeValueWithNewtonsoft()
    .UseConfiguration<Configuration>());
```

If you want to use custom deserializer, you can call `DeserializeKeyWith<T> where T : IDeserializer<T>` method,
same way with value deserialization.

If your topic model is defined as protobuf, you can call `DeserializeKeyWithProto` extension method,
same way with value deserialization.

## Producer

### Add producer to your service collection

```csharp
collection.AddKafkaProducer<int, string>(builder => builder
    .SerializeKeyWithNewtonsoft()
    .SerializeValueWithNewtonsoft()
    .UseConfiguration<Configuration>());
```

You can use protobuf serialization same way as it is with consumer.

### Resolve producer instance from service provider

```csharp
var producer = provider.GetRequiredService<IKafkaMessageProducer<int, string>>();
await producer.ProduceAsync(messages, default);
```