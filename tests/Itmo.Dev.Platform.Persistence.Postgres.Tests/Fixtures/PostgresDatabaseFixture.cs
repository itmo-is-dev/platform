using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.Persistence.Abstractions.Connections;
using Itmo.Dev.Platform.Persistence.Abstractions.Extensions;
using Itmo.Dev.Platform.Persistence.Postgres.Extensions;
using Itmo.Dev.Platform.Testing.Fixtures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Persistence.Postgres.Tests.Fixtures;

public class PostgresDatabaseFixture : DatabaseFixture
{
    public AsyncServiceScope Scope => Provider.CreateAsyncScope();

    protected override void ConfigureServices(IServiceCollection collection)
    {
        var configurationValues = new Dictionary<string, string?>
        {
            { "PostgresConfiguration:Host", Container.Hostname },
            { "PostgresConfiguration:Port", Container.GetMappedPublicPort(5432).ToString() },
            { "PostgresConfiguration:Database", "postgres" },
            { "PostgresConfiguration:Username", "postgres" },
            { "PostgresConfiguration:Password", "postgres" },
            { "PostgresConfiguration:SslMode", "Prefer" },
        };

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();

        collection.AddSingleton<IConfiguration>(configuration);

        collection.AddPlatform();

        collection.AddPlatformPersistence(
            persistence => persistence
                .UsePostgres(
                    postgres => postgres
                        .WithConnectionOptions(builder => builder.BindConfiguration("PostgresConfiguration"))
                        .WithMigrationsFrom()));
    }

    protected override async ValueTask UseProviderAsync(IServiceProvider provider)
    {
        const string sql = """
        create table if not exists placeholder();
        """;

        var connectionProvider = provider.GetRequiredService<IPersistenceConnectionProvider>();
        var connection = await connectionProvider.GetConnectionAsync(default);

        await using var command = connection.CreateCommand(sql);
        await command.ExecuteNonQueryAsync(default);
    }
}