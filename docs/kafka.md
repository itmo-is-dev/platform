# Itmo.Dev.Platform.Kafka

Библиотека для работы с Kafka. Включает в себя абстракции для продюсера и консьюмера, аутбокса и инбокса.

## Подключение

Библиотека состоит из Nuget пакетов: `Itmo.Dev.Platform.Kafka` и `Itmo.Dev.Platform.MessagePersistence`
(в случае использования аутбокса и/или инбокса). 

Для регистрации в DI-контейнере необходимо вызвать метод `AddPlatformKafka`, предварительно зарегистрировав саму
платформу (метод `AddPlatform`, в нём необходимо выбрать сериализатор).
Метод `AddPlatformKafka` принимает делегат, который настраивает билдер. Этот билдер настраивает конфигурацию, добавлять продюсеры и консьюмеры.

```csharp
collection.AddPlatformKafka(builder => builder
    .ConfigureOptions(configuration.GetSection("Presentation:Kafka")));
```

## Конфигурация

Параметры Kafka задаются через `PlatformKafkaOptions`. Схема конфигурации:

```json
{
  "Host": string,
  "SecurityProtocol": string enum,
  "SslCaPem": string,
  "SaslMechanism": string enum,
  "SaslUsername": string,
  "SaslPassword": string
}
```

- **Host**  
  Bootstrap servers кластера Kafka.
- **SecurityProtocol**  
  Перечисление, определяющее протокол безопасности, используемый для подключения к кластеру Kafka. Допустимые значения: `Plaintext` (по умолчанию), `Ssl`, `SaslPlaintext`, `SaslSsl`).
- **SslCaPem**  
  Строковое представление CA сертификата. Требуется, если `SecurityProtocol` установлен в `Ssl` или `SaslSsl`.
- **SaslMechanism**  
  Перечисление, определяющее механизм `Sasl`, используемый для подключения к кластеру Kafka. Допустимые значения: `Gssapi`, `Plain`, `ScramSha256`, `ScramSha512`, `OAuthBearer`.
  Используется, если SecurityProtocol установлен в `SaslPlaintext` или `SaslSsl`, необязательный параметр.
- **SaslUsername**  
  Имя пользователя, используемое для авторизации при подключении к кластеру Kafka. Требуется, если SecurityProtocol установлен в `SaslPlaintext` или `SaslSsl`.
- **SaslPassword**  
  Пароль, используемый для авторизации при подключении к кластеру Kafka. Требуется, если SecurityProtocol установлен в `SaslPlaintext` или `SaslSsl`.

### Варианты перечисления SecurityProtocol

- **Plaintext**  
  Дополнительная настройка безопасности не требуется.
- **Ssl**  
  Требует `SslCaPem`.
- **SaslPlaintext**  
  Требует `SaslUsername` и `SaslPassword`.
- **SaslSsl**  
  Требует `SaslUsername`, `SaslPassword` и `SslCaPem`.

Пример конфигурации:

```json
{
  "Presentation": {
    "Kafka": {
      "Host": "localhost:8001",
      "SecurityProtocol": "SaslSsl",
      "SslCaPem": "...",
      "SaslMechanism": "ScramSha512",
      "SaslUsername": "username",
      "SaslPassword": "password"
    }
  }
}
```

## Консьюм

Для обработки сообщений из топика необходимо реализовать интерфейс `IKafkaConsumer<,>`.
Первый generic-параметр обозначает тип ключа сообщения, а второй – тип тела сообщения.

```csharp
public ValueTask HandleAsync(
    IEnumerable<IKafkaConsumerMessage<MessageKey, MessageValue>> messages,
    CancellationToken cancellationToken)
```

### Сообщение из топика

`IKafkaConsumerMessage<,>` содержит свойства:

- **Key**  
  Ключ сообщения.
- **Value**  
  Тело сообщения.
- **Timestamp**   
  Время (`DateTimeOffset`), когда сообщение было отправлено в Kafka.
- **Topic**  
  Название топика, из которого сообщение было вычитано.
- **Partition**  
  Партиция, из которой сообщение было вычитано.
- **Offset**  
  Порядковый номер сообщения внутри его партиции.

Пример работы с консьюмером:

```csharp
public class ConsumerHandler : IKafkaConsumerHandler<MessageKey, MessageValue>
{
    public ValueTask HandleAsync(
        IEnumerable<IKafkaConsumerMessage<MessageKey, MessageValue>> messages,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Consumer received {messages.Count()} messages");
        return ValueTask.CompletedTask;
    }
} 
```

### Регистрация и конфигурация 

Чтобы зарегистрировать консьюмер, необходимо использовать тот же билдер, который предоставляется в методе `AddPlatformKafka`.

Необходимо вызвать метод `AddConsumer` в цепочке вызовов билдера, после вызова `ConfigureOptions` (или другого `AddConsumer`, `AddProducer`). 
Он принимает делегат, используемый для настройки билдера консьюмера.

Необходимо указать тип ключа, тип тела сообщения, конфигурацию, десериализаторы ключа и тела, а также хендлер.
Метод `HandleWith<>` принимает в качестве параметра типа класс, реализующий `IKafkaConsumerHandler<,>`.

```csharp
collection.AddPlatformKafka(builder => builder
    .ConfigureOptions(...)
    .AddConsumer(b => b
        .WithKey<MessageKey>()
        .WithValue<MessageValue>()
        .WithConfiguration(
            configuration.GetSection("Presentation:Kafka:Consumers:ConsumerName"),
            c => c.WithGroup(group))
        .DeserializeKeyWithNewtonsoft()
        .DeserializeValueWithNewtonsoft()
        .HandleWith<MessageHandler>()));
```

Конфигурация происходит через `KafkaConsumerOptions`, используется как `IOptionsMonitor<>`. Схема конфигурации:

```json
{
    "IsDisabled": bool,
    "Topic": string,
    "Group": string,
    "InstanceId": string,
    "ParallelismDegree": int,
    "BufferSize": int,
    "BufferWaitLimit": string timespan,
    "ReadLatest": bool
}
```
- **IsDisabled**  
  Определяет, отключён ли consumer.
- **Topic**  
  Название топика, из которого необходимо читать сообщения. Обязательный параметр.
  Изменение этой опции во время выполнения приложения приведёт к неопределённому поведению.
- **Group**  
  Имя консьюмер группы, используемое при подключении к кластеру Kafka. Обязательный параметр.
- **InstanceId**  
  Id экземпляра в указанной консьюмер группы. Необязательный параметр, по умолчанию — пустая строка.
- **ParallelismDegree**  
  Количество параллельных потоков, которые будут обрабатывать сообщения. Необязательный параметр, по умолчанию — 1, должен быть не менее 1.
- **BufferSize**  
  Хендлер консьюмера получает сообщения батчами, параметр используется для настройки максимального размера батча. 
  Необязательный параметр, по умолчанию — 1, должен быть не менее 1.
- **BufferWaitLimit**  
  Во избежание голодания (starvation) консьюмера при батчинге, можно настроить максимальное время ожидания накопления одного батча. 
  Если время ожидания одного батча превышает этот порог, будет использоваться неполный батч. Обязательный параметр, если BufferSize больше 1, должен быть больше `TimeSpan.Zero`.
- **ReadLatest**  
  Настраивает стратегию выбора offset для первой подписки консьюмера на топик. Если true, offset по умолчанию будет установлен на последнее сообщение в топике; 
  если false, offset будет установлен на первое сообщение в топике. Необязательный параметр, по умолчанию = false.

Пример конфигурации:

```json
{
  "Presentation": {
    "Kafka": {
      "Consumers": {
        "MessageName": {
          "IsDisabled": false,
          "Topic": "my_topic",
          "ParallelismDegree": 2,
          "BufferSize": 100,
          "BufferWaitLimit": "00:00:01.500",
          "ReadLatest": false
        },
      },
    }
  }
}
```

## Inbox консьюмер

> Перед настройкой Kafka инбокса небходимо настроить `Itmo.Dev.Platform.MessagePersistence`.

Для настройки MessagePersistense необходимо вызвать метод расширения
`AddPlatformMessagePersistence`, куда передается билдер. У этого билдера необходимо вызвать методы `WithDefaultPublisherOptions` и  `UsePostgresPersistence`, передав параметры конфигурации
для настройки MessagePersistense. Пример настройки:

```csharp
collection.AddPlatformMessagePersistence(persistence => persistence
    .WithDefaultPublisherOptions("Infrastructure:MessagePersistence:Publishers:Default")
    .UsePostgresPersistence(postgres => postgres
        .ConfigureOptions(options => options.BindConfiguration("Infrastructure:MessagePersistence:Persistence")));
```

Пример конфигурации для MessagePersistense:

```json
{
  "MessagePersistence": {
    "Persistence": {
      "SchemaName": "message_persistence"
    },
    "Publishers": {
      "Default": {
        "BatchSize": 100,
        "PollingDelay": "00:00:00.500"
      }
    }
  }
}
```

Чтобы использовать инбокс с Kafka консьюмером, необходимо использовать метод `HandleInboxWith<>` вместо `HandleWith<>` 
и передать тип, реализующий `IKafkaInboxHandler<,>`, в его generic-параметр.

Пример регистрации:

```csharp
collection.AddPlatformKafka(builder => builder
    .ConfigureOptions(...)
    .AddConsumer(b => b
        .WithKey<MessageKey>()
        .WithValue<MessageValue>()
        .WithConfiguration(...)
        .DeserializeKeyWithNewtonsoft()
        .DeserializeValueWithNewtonsoft()
        .HandleInboxWith<InboxMessageHandler>()));
```

Для настройки инбокса, необходимо добавить подраздел `Inbox` в конфигурацию, которую передеётся в билдер консьюмера.

### Конфигурация

Схема конфигурации:

```json
{
  "BatchSize": int,
  "PollingDelay": string timespan,
  "DefaultHandleResult": string enum
}
```

- **BatchSize**  
  Максимальное количество сообщений, которое inbox processor будет забирать из базы данных. Обязательный параметр, должен быть не менее 1.
- **PollingDelay**  
  Задержка между каждым опросом (polls), который будет выполнять inbox processor. Обязательный параметр, должен быть более `TimeSpan.Zero`.
- **DefaultHandleResult**  
  Результат обработки сообщения инбокса, когда результат не был явно установлен. Необязательный параметр, по умолчанию = `Success`.

Пример конфигурации:

```json
{
  "Presentation": {
    "Kafka": {
      "Consumers": {
        "MessageName": {
          ...
          "Inbox": {
            "BatchSize": 100,
            "PollingDelay": "00:00:00.500",
            "DefaultHandleResult": "Ignored"
          }
        },
      },
    }
  }
}
```

## Продюс

Для отправки сообщений в топик необходимо использовать в коде хендлера интерфейс `IKafkaMessageProducer<,>`, 
который извлекается из DI контейнера.
Первый generic-параметр обозначает тип ключа сообщения, а второй – тип тела сообщения.

Для отправки сообщений используется метод `ProduceAsync`. 
Он принимает `IAsyncEnumerable<KafkaConsumerMessage<,>>` и имеет перегрузку для одного `KafkaConsumerMessage<,>`.

Пример использования:

```csharp
public class UsageScenario
{
    private readonly IKafkaMessageProducer<MessageKey, MessageValue> _producer;

    public UsageScenario(IKafkaMessageProducer<MessageKey, MessageValue> producer)
    {
        _producer = producer;
    }

    public async Task PublishMessagesAsync(IEnumerable<Message> messages, CancellationToken cancellationToken)
    {
        var producerMessages = messages
            .Select(message => new KafkaProducerMessage<MessageKey, MessageValue>(message.Key, message.Value))
            .ToAsyncEnumerable();

        await _producer.ProduceAsync(producerMessages, cancellationToken);
    }
}
```

### Регистрация и конфигурация

Чтобы зарегистрировать продюсер, необходимо использовать тот же билдер, который предоставляется в методе `AddPlatformKafka`.

Необходимо вызвать метод `AddProducer` в цепочке вызовов билдера, после вызова `ConfigureOptions` (или другого `AddConsumer`, `AddProducer`).
Он принимает делегат, используемый для настройки билдера продюсера.

Необходимо указать тип ключа, тип тела сообщения, конфигурацию, а также cериализаторы ключа и тела.

```csharp
collection.AddPlatformKafka(builder => builder
    .ConfigureOptions(...)
    .AddProducer(b => b
        .WithKey<MessageKey>()
        .WithValue<MessageValue>()
        .WithConfiguration(configuration.GetSection("Presentation:Kafka:Producers:MessageName"))
        .SerializeKeyWithNewtonsoft()
        .SerializeValueWithNewtonsoft()));
```

Конфигурация происходит через `KafkaProducerOptions`, используется как `IOptionsMonitor<>`. Схема конфигурации:

```json
{
  "Topic": string,
  "MessageMaxBytes": int
}
```

- **Topic**  
  Название топика, в который производится продюс. Обязательный параметр.
- **MessageMaxBytes**  
  Максимальный размер сообщения в байтах, которое продюсер может отправить в топик. Необязательный параметр, по умолчанию = `1_000_000`.
  Должен быть не менее 1.

Пример конфигурации:

```json
{
  "Presentation": {
    "Kafka": {
      "Producers": {
        "MessageName": {
          "Topic": "my_topic",
          "MessageMaxBytes": 2000000
        },
      },
    }
  }
}
```

## Outbox продюсер

> Перед настройкой Kafka аутбокса небходимо настроить `Itmo.Dev.Platform.MessagePersistence`. Как это сделать описано [тут](#inbox-консьюмер)

Чтобы использовать аутбокс с Kafka консьюмером, необходимо использовать метод `WithOutbox` в билдере для продюсера.

Пример регистрации:

```csharp
collection.AddPlatformKafka(builder => builder
    .ConfigureOptions(...)
    .AddProducer(b => b
        .WithKey<MessageKey>()
        .WithValue<MessageValue>()
        .WithConfiguration(configuration.GetSection("Presentation:Kafka:Producers:MessageName"))
        .SerializeKeyWithNewtonsoft()
        .SerializeValueWithNewtonsoft())
        .WithOutbox());
```

Для настройки инбокса, необходимо добавить подраздел `Outbox` в конфигурацию, которая передаётся в билдер продюсера.

### Конфигурация

Схема конфигурации:

```json
{
  "BatchSize": int,
  "PollingDelay": string timespan,
  "DefaultHandleResult": string enum,
  "Strategy": string enum
}
```

- **BatchSize**  
  Максимальное количество сообщений, которое outbox processor будет забирать из базы данных. Обязательный параметр, должен быть не менее 1.
- **PollingDelay**  
  Задержка между каждым опросом (polls), который будет выполнять inbox processor. Обязательный параметр, должен быть более `TimeSpan.Zero`.
- **DefaultHandleResult**  
  Результат обработки outbox message, когда результат не был явно установлен. Необязательный параметр, по умолчанию = `Success`.
- **Strategy**  
  Стратегия использования outbox. `Always` – сообщения всегда будут помещаться в outbox перед отправкой. 
  `Fallback` – сообщения будут помещаться в outbox только в случае, если прямая отправка в топик Kafka не удалась. 
  Необязательный параметр, по умолчанию = `Always`.

Пример конфигурации:

```json
{
  "Presentation": {
    "Kafka": {
      "Consumers": {
        "MessageName": {
          ...
          "Inbox": {
            "BatchSize": 100,
            "PollingDelay": "00:00:00.500",
            "DefaultHandleResult": "Ignored"
          }
        },
      },
    }
  }
}
```