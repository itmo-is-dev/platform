using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Postgres.Configuration;

internal class MessagePersistencePostgresConfigurator :
    PostgresConfig.IOptionsStep,
    PostgresConfig.IFinalStep
{
    private readonly IServiceCollection _collection;

    public MessagePersistencePostgresConfigurator(IServiceCollection collection)
    {
        _collection = collection;
    }

    public PostgresConfig.IFinalStep ConfigureOptions(
        Action<OptionsBuilder<MessagePersistencePostgresOptions>> action)
    {
        var builder = _collection
            .AddOptions<MessagePersistencePostgresOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        action.Invoke(builder);

        return this;
    }
}
