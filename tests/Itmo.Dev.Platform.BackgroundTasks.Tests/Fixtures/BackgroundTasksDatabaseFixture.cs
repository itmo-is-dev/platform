using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.Persistence.Abstractions.Extensions;
using Itmo.Dev.Platform.Persistence.Postgres.Extensions;
using Itmo.Dev.Platform.Testing.Fixtures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Fixtures;

public class BackgroundTasksDatabaseFixture : DatabaseFixture
{
    public AsyncServiceScope Scope => Provider.CreateAsyncScope();

    public void AddPlatformPersistence(IServiceCollection collection)
    {
        collection.AddPlatformPersistence(
            persistence => persistence.UsePostgres(
                postgres => postgres
                    .WithConnectionOptions(
                        b => b.Configure(
                            options =>
                            {
                                options.Host = Container.Hostname;
                                options.Port = Container.GetMappedPublicPort(5432);
                                options.Database = "postgres";
                                options.Username = "postgres";
                                options.Password = "postgres";
                                options.SslMode = "Prefer";
                            }))
                    .WithMigrationsFrom()));
    }

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

        collection.AddPlatform();

        collection.AddPlatformPersistence(
            persistence => persistence.UsePostgres(
                postgres => postgres
                    .WithConnectionOptions(b => b.BindConfiguration("PostgresConfiguration"))
                    .WithMigrationsFrom()));
    }

    protected override async ValueTask UseProviderAsync(IServiceProvider provider)
    {
        const string sql = """
        create table if not exists placeholder();
        """;

        await using var command = new NpgsqlCommand(sql, Connection);
        await command.ExecuteNonQueryAsync();
    }
}