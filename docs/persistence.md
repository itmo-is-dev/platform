## Itmo.Dev.Platform.Persistence

Библиотека для работы с базами данных. Включает в себя абстракции для подключения к БД, выполнения запросов, управления транзакциями и миграциями.

## Подключение

Библиотека состоит из двух Nuget пакетов: `Itmo.Dev.Platform.Persistence.Abstractions` с абстракциями и
`Itmo.Dev.Platform.Persistence.Postgres` с реализациями для БД Postgres.

Для регистрации в DI-контейнере необходимо вызвать метод `AddPlatformPersistence`. 
Метод принимает делегат, в котором происходит конфигурация Persistence слоя.

```csharp
collection.AddPlatformPersistence(persistence => persistence.UsePostgres(postgres => postgres
    .WithConnectionOptions(static builder => builder
        .BindConfiguration("Infrastructure:DataAccess:PostgresConfiguration"))
    .WithMigrationsFrom(typeof(IAssemblyMarker).Assembly)
    .WithDataSourcePlugin<MappingPlugin>()));
```

У конфигуратора есть метод расширения `UsePostgres`, который настраивает подключение к PostgreSql. В метод так же передаётся 
делегат, в котором можно параметризовать работу с базой с помощью методов:

- `WithConnectionOptions` - настройка конфигурации подключения к БД.
- `WithMigrationsFrom` - указание сборки(-ок) с миграциями. Также можно использовать `WithMigrationsFromItems` для указания конкретных типов
с миграциями.
- `WithDataSourcePlugin` - подключение плагинов для `NpgsqlDataSource` (например, кастомный маппинг enum-ов).

## Конфигурация

Параметры подключения к БД задаются через `PostgresConnectionOptions`.

### Схема конфигурации

```json
{
  "Host": string,
  "Port": int,
  "Database": string,
  "Username": string,
  "Password": string,
  "SslMode": string,
  "Pooling": bool,
  "MaximumPoolSize": int,
  "EnableConnectionProviderLogging": bool
}
```

- **Host**  
  Адрес хоста PostgreSql. Обязательный параметр, не может быть пустым.
- **Port**  
  Порт PostgreSql. Обязательный параметр, не может быть пустым или равным 0. Допустимые значения: `1`-`65535`.
- **Database**  
  Название базы данных. Обязательный параметр, не может быть пустым.
- **Username**  
  Имя пользователя. Обязательный параметр, не может быть пустым.
- **Password**  
  Пароль. Обязательный параметр, не может быть пустым.
- **SslMode**  
  Режим SSL-соединения. Необязательный параметр, по умолчанию пустая строка. 
  Допустимые значения аналогичны Npgsql ([документация](https://www.npgsql.org/doc/api/Npgsql.SslMode.html)): `Disable`, `Allow`, `Prefer`, `Require`, `VerifyCA`, `VerifyFull`.
- **Pooling**  
  Использование пула соединений (connection pooling). Необязательный параметр, по умолчанию `true`.
- **MaximumPoolSize**  
  Максимальный размер пула соединений. Необязательный параметр, по умолчанию `10`, должен быть >= `1`.
- **EnableConnectionProviderLogging**  
  Включение логирования. Необязательный параметр, по умолчанию `false`.

> Валидация значений происходит при старте сервиса. При невалидных значениях сервис не запустится.

### Configuration example

```json
{
  "Infrastructure": {
    "DataAccess": {
      "PostgresConfiguration": {
        "Host": "localhost",
        "Database": "test-db",
        "Port": 6432,
        "Username": "postgres",
        "Password": "postgres",
        "SslMode": "Prefer",
        "Pooling": true,
        "MaximumPoolSize": 10,
        "EnableConnectionProviderLogging": false
      }
    }
  }
}
```

## Подключение к БД и выполнение запросов

### Получение соединения

Для взаимодействия с БД используется `IPersistenceConnectionProvider`. 
Провайдер зарегистрирован в DI как Scoped, поэтому сервисы использующие его должны быть зарегистрированы как Scoped или Transient.

Чтобы получить объект соединения, необходимо вызвать метод провайдера `GetConnectionAsync`, который возвращает 
`IPersistenceConnection`.   

```csharp
await using IPersistenceConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);
```

### Создание и настройка команды

Команда создается через метод `CreateCommand` на объекте соединения. Параметры добавляются в fluent стиле:

```csharp
await using IPersistenceCommand command = connection.CreateCommand(sql)
    .AddParameter("parameter_1", value1)
    .AddParameter("parameter_2", value2);
```

Помимо вышеуказанного `AddParameter<T>(string parameterName, T value)`, у `IPersistenceConnection` существуют другие методы и перегрузки для добавления параметров: 

- `AddParameter(DbParameter parameter)` - добавляет сырой `DbParameter`, фактически параметр должен быть типа `NpgsqlParameter`.
- `AddParameter<T>(string parameterName, T value)` - добавляет именованный параметр произвольного типа.
- `AddParameter<T>(string parameterName, IEnumerable<T> values)` - добавляет именованный параметр-коллекцию, используется для параметров-списков. 
  В sql коде удобно подставлять в `= any(...)` или `in (...)`.
- `AddJsonParameter<T>(string parameterName, T value, JsonSerializerSettings? serializerSettings = null)` - сериализует переданное значение в json и добавляет как параметр с типом `jsonb`.
- `AddNullableJsonParameter<T>(string parameterName, T? value, JsonSerializerSettings? serializerSettings = null)` - аналогично `AddJsonParameter`, но поддерживает `null` значения.
- `AddJsonArrayParameter<T>(string parameterName, IEnumerable<T> values, JsonSerializerSettings? serializerSettings = null)` - сериализует каждый элемент коллекции в json и добавляет как параметр с типом `jsonb[]`.
- `AddJsonArrayParameter(string parameterName, IEnumerable<string> values)` - добавляет коллекцию строк (уже сериализованных объектов) как параметр с типом `jsonb[]`.
- `AddMultiArrayStringParameter<T>(string parameterName, IEnumerable<IEnumerable<T>> values)` - позволяет добавлять многомерные массивы в качестве параметров. 
  Каждый элемент коллекции 1-го уровня преобразуется в postgres-массив, затем итоговый массив добавляется как параметр с типом `text[]`.
- `AddMultiArrayStringParameter(string parameterName, IEnumerable<IEnumerable<string>> values)` - аналогично `AddMultiArrayStringParameter`, но для строковых значений коллекции.

> При попытке добавить параметр с уже существующим названием будет выброшен `PlatformPersistencePostgresException`.

### Выполнение запросов

Чтение данных через реализовано через метод команды `ExecuteReaderAsync`, аналогично `NpgsqlCommand`:

```csharp
await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

while (await reader.ReadAsync(cancellationToken))
{
    yield return new Model(
        Id: reader.GetInt64("field_1"),
        Name: reader.GetString("field_2"));
}
```

Выполнение не читающих запросов (DML) реализовано через метод `ExecuteNonQueryAsync`, так же аналогично `NpgsqlCommand`:

```csharp
await command.ExecuteNonQueryAsync(cancellationToken);
```

## Транзакции

Для управления транзакциями используется `IPersistenceTransactionProvider`, зарегистрированный в DI как Scoped.
Соответственно сервисы, использующие его, должны быть зарегистрированы как Scoped или Transient.

Для открытия транзакции используется метод `BeginTransactionAsync` с указанием уровня изоляции. 
Метод возвращает `IPersistenceTransaction` - объект транзакции, который после выполнения операций необходимо закоммитить
(вызвать метод `CommitAsync`). Все операции, выполненные до вызова этого метода, автоматически попадают в транзакцию.

Если `CommitAsync` не был вызван, транзакция откатывается при `DisposeAsync`.

```csharp
await using IPersistenceTransaction transaction = await _transactionProvider
    .BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

// выполнение операций через репозитории

await transaction.CommitAsync(cancellationToken);
```

Фактически реализация `IPersistenceTransaction` использует под капотом `TransactionScope` с указанием параметра scopeOption: `TransactionScopeOption.Required`,
тем самым представляя собой ту же ambient транзакцию.

## Миграции

Механизм миграций реализован на базе библиотеки FluentMigrator. 
Для написания миграции необходимо наследовать абстрактный класс `SqlMigration`, в котором определены методы `GetUpSql` и `GetUpSql`.

`GetUpSql` - sql-скрипт самой миграции, `GetDownSql` - скрипт отката 

```csharp
[Migration(1, "Initial")]
public class Initial : SqlMigration
{
    protected override string GetUpSql(IServiceProvider serviceProvider)
    {
        return """
        create table example
        (
            id bigserial not null primary key,
            name text not null
        );
        """;
    }

    protected override string GetDownSql(IServiceProvider serviceProvider)
    {
        return """
        drop table example;
        """;
    }
}
```

Миграции запускаются автоматически при старте приложения. Сборка с миграциями указывается при регистрации в DI, методе `WithMigrationsFrom`.

// TODO: возможно что-то написать про WithMigrationsFromItems 
