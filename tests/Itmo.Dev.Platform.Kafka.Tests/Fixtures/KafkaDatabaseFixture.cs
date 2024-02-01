using Itmo.Dev.Platform.Postgres.Connection;
using Itmo.Dev.Platform.Postgres.Extensions;
using Itmo.Dev.Platform.Testing.Fixtures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Itmo.Dev.Platform.Kafka.Tests.Fixtures;

public class KafkaDatabaseFixture : DatabaseFixture
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

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();

        collection.AddSingleton<IConfiguration>(configuration);
        collection.AddPlatformPostgres(builder => builder.BindConfiguration("PostgresConfiguration"));
    }

    protected override async ValueTask UseProviderAsync(IServiceProvider provider)
    {
        const string sql = """
        create table if not exists placeholder();
        """;

        var connectionProvider = provider.GetRequiredService<IPostgresConnectionProvider>();
        var connection = await connectionProvider.GetConnectionAsync(default);

        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}