using Itmo.Dev.Platform.MessagePersistence.Persistence;
using Itmo.Dev.Platform.MessagePersistence.Persistence.Migrations;
using Itmo.Dev.Platform.MessagePersistence.Persistence.Plugins;
using Itmo.Dev.Platform.MessagePersistence.Persistence.Queries;
using Itmo.Dev.Platform.MessagePersistence.Persistence.Repositories;
using Itmo.Dev.Platform.MessagePersistence.Services;
using Itmo.Dev.Platform.Postgres.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.MessagePersistence.Configuration.Builders;

internal class MessagePersistenceConfigurationBuilder :
    IMessagePersistencePersistenceConfigurator,
    IMessagePersistenceConfigurationBuilder
{
    private readonly IServiceCollection _collection;

    public MessagePersistenceConfigurationBuilder(IServiceCollection collection)
    {
        _collection = collection;
    }

    public IMessagePersistenceConfigurationBuilder ConfigurePersistence(
        IConfiguration configuration,
        Action<MessagePersistencePersistenceOptions>? action = null)
    {
        var builder = _collection
            .AddOptions<MessagePersistencePersistenceOptions>()
            .Bind(configuration)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        if (action is not null)
            builder.Configure(action);

        _collection.AddSingleton<MessagePersistenceQueryFactory>();
        _collection.AddSingleton<MessagePersistenceQueryStorage>();

        _collection.AddSingleton<IDataSourcePlugin, MappingPlugin>();
        _collection.AddHostedService<MigrationRunnerService>();

        _collection.AddScoped<MessagePersistenceRepository>();
        _collection.AddScoped<IMessagePersistenceInternalRepository>(p
            => p.GetRequiredService<MessagePersistenceRepository>());

        _collection.AddScoped<IMessagePersistenceConsumer, MessagePersistenceConsumer>();

        return this;
    }

    public IMessagePersistenceConfigurationBuilder AddMessage(
        Func<IMessagePersistenceHandlerNameConfigurator, IMessagePersistenceHandlerBuilder> configuration)
    {
        var configurator = new MessagePersistenceHandlerNameConfigurator(_collection);
        configuration.Invoke(configurator).Build();

        return this;
    }
}