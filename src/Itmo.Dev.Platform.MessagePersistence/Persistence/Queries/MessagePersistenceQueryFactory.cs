using Itmo.Dev.Platform.MessagePersistence.Configuration;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Persistence.Queries;

internal class MessagePersistenceQueryFactory
{
    private readonly IOptionsMonitor<MessagePersistencePersistenceOptions> _options;

    public MessagePersistenceQueryFactory(IOptionsMonitor<MessagePersistencePersistenceOptions> options)
    {
        _options = options;
    }

    public MessagePersistenceQuery Create(Func<MessagePersistencePersistenceOptions, string> factory)
    {
        return new MessagePersistenceQuery(factory, _options);
    }
}