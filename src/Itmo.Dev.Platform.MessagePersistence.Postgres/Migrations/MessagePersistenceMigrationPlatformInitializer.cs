using FluentMigrator.Runner;
using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.Common.Lifetime.Initializers;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Configuration;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Plugins;
using Itmo.Dev.Platform.Persistence.Abstractions.Extensions;
using Itmo.Dev.Platform.Persistence.Postgres.Extensions;
using Itmo.Dev.Platform.Persistence.Postgres.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Postgres.Migrations;

public class MessagePersistenceMigrationPlatformInitializer : PlatformLifetimeInitializerBase
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MessagePersistenceMigrationPlatformInitializer(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await using var outerScope = _scopeFactory.CreateAsyncScope();

        var collection = new ServiceCollection();

        var connectionOptions = outerScope.ServiceProvider.GetRequiredService<IOptions<PostgresConnectionOptions>>();

        var persistenceOptions = outerScope.ServiceProvider
            .GetRequiredService<IOptions<MessagePersistencePostgresOptions>>();

        collection
            .AddOptions<MessagePersistencePostgresOptions>()
            .Configure(o => persistenceOptions.Value.ApplyTo(o));

        collection.AddPlatform();

        collection.AddPlatformPersistence(
            persistence => persistence.UsePostgres(
                postgres => postgres
                    .WithConnectionOptions(builder => builder.Configure(o => connectionOptions.Value.ApplyTo(o)))
                    .WithMigrationsFromItems(new MigrationSourceItem())
                    .WithDataSourcePlugin<MappingPlugin>()));

        var provider = collection.BuildServiceProvider();
        await using var innerScope = provider.CreateAsyncScope();

        var runner = innerScope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }
}