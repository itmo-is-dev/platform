using Itmo.Dev.Platform.MessagePersistence.Postgres.Configuration;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Postgres.Queries;

internal class MessagePersistenceQueryFactory
{
    private readonly IOptionsMonitor<MessagePersistencePostgresOptions> _options;

    public MessagePersistenceQueryFactory(IOptionsMonitor<MessagePersistencePostgresOptions> options)
    {
        _options = options;
    }

    public MessagePersistenceQuery Create(Func<MessagePersistencePostgresOptions, string> factory)
    {
        return new MessagePersistenceQuery(factory, _options);
    }
}