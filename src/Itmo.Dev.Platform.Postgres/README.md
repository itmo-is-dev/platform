# Itmo.Dev.Platform.Postgres

Platform extensions for working with PostgreSQL database

## Connection

You can obtain database connection using `IPostgresConnectionProvider`

```csharp
var connection = await provider.GetConnectionAsync(cancellationToken);
```

## Migrations

Package using FluentMigrator as it's migration engine,
thus all FluentMigrator's migrations are compatible and will be executed.

Nevertheless, it is strongly recommended to inherit from `SqlMigration` and implement your migrations as raw sql queries.

```csharp
[Migration(1, "Initial")]
public class InitialMigration : SqlMigration
{
    protected override string GetUpSql(IServiceProvider serviceProvider)
    {
        return """
        CREATE TABLE USERS
        (
            user_id bigint PRIMARY KEY,
            user_name text NOT NULL
        );
        """;
    }
    
    protected override string GetDownSql(IServiceProvider serviceProvider)
    {
        return """
        DROP TABLE USERS;
        """;
    }
}
```

## Registration

Use extension methods on service collection to register Itmo.Dev.Platform.Postgres services and migrations.

### Connection

To configure connection string bind configuration with such schema to configuration builder, 
using extension method's parameter.

```json
{
  "Host" : "string",
  "Port" : 5432,
  "Database" : "string",
  "Username" : "string",
  "Password" : "string",
  "SslMode" : "string",
  "Pooling" : true
}
```

```csharp
collection.AddPlatformPostgres(options => options.Bind("Database"));
```


### Migrations

To register migrations call `AddPlatformMigrations` on service collection. Pass the assemblies containing migration
classes.

```csharp
collection.AddPlatformMigrations(typeof(IMigrationsAssemblyMarker).Assembly);
```

To run migrations, create scope and call `UsePlatformMigrationsAsync` on it.

```csharp
using var scope = serviceProvider.CreateScope();
await scope.UsePlatformMigrationsAsync()
```