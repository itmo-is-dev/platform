using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Serilog;
using System.Data;
using Testcontainers.PostgreSql;
using Xunit;

namespace Itmo.Dev.Platform.Testing.Fixtures;

public abstract class DatabaseFixture : IAsyncLifetime
{
    private const string User = "postgres";
    private const string Password = "postgres";
    private const string Database = "postgres";

    private readonly bool _createRespawnerOnInitialization;

    private Respawner _respawn;

    // ReSharper disable once ConvertConstructorToMemberInitializers
    protected DatabaseFixture(bool createRespawnerOnInitialization = true)
    {
        _createRespawnerOnInitialization = createRespawnerOnInitialization;

        var containerBuilder = new PostgreSqlBuilder("postgres:latest")
            .WithUsername(User)
            .WithPassword(Password)
            .WithDatabase(Database);

        // ReSharper disable once VirtualMemberCallInConstructor
        containerBuilder = ConfigurePostgresContainer(containerBuilder);

        Container = containerBuilder.Build();

        Connection = null!;
        Provider = null!;
        _respawn = null!;
    }

    public NpgsqlConnection Connection { get; private set; }

    protected PostgreSqlContainer Container { get; }

    protected ServiceProvider Provider { get; private set; }

    public virtual async Task ResetAsync()
    {
        bool wasOpen = Connection.State is ConnectionState.Open;

        if (wasOpen is false)
        {
            await Connection.OpenAsync();
        }

        await _respawn.ResetAsync(Connection);

        if (wasOpen is false)
        {
            await Connection.CloseAsync();
        }
    }

    public async Task InitializeAsync()
    {
        await Container.StartAsync();

        var collection = new ServiceCollection();
        collection.AddLogging(x => x.AddSerilog());

        ConfigureServices(collection);

        Provider = collection.BuildServiceProvider();
        Connection = CreateConnection();

        await UseProviderAsync(Provider);

        if (_createRespawnerOnInitialization)
            await CreateRespawnerAsync();
    }

    public virtual async Task DisposeAsync()
    {
        await Connection.DisposeAsync();
        await Container.DisposeAsync();
        await Provider.DisposeAsync();
    }

    public async Task CreateRespawnerAsync()
    {
        RespawnerOptions options = GetRespawnOptions();

        var oldState = Connection.State;

        if (oldState is not ConnectionState.Open)
            await Connection.OpenAsync();

        _respawn = await Respawner.CreateAsync(Connection, options);

        if (oldState is not ConnectionState.Open)
            await Connection.CloseAsync();
    }

    protected virtual void ConfigureServices(IServiceCollection collection) { }

    protected virtual PostgreSqlBuilder ConfigurePostgresContainer(PostgreSqlBuilder builder)
    {
        return builder;
    }

    protected virtual ValueTask UseProviderAsync(IServiceProvider provider)
    {
        return ValueTask.CompletedTask;
    }

    protected virtual RespawnerOptions GetRespawnOptions()
    {
        return new RespawnerOptions
        {
            SchemasToInclude = ["public"],
            DbAdapter = DbAdapter.Postgres,
        };
    }

    protected virtual NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(Container.GetConnectionString());
    }
}
